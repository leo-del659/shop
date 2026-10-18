using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.Infrastructure.MQ
{
    /// <summary>
    /// RabbitMQ 连接管理器（单例）。
    /// 懒连接 + 自动恢复（AutomaticRecoveryEnabled），任何一步失败都不抛异常，
    /// 由 TryCreateModel 返回 null 供上层走进程内降级链路，保证业务主流程永不被 MQ 拖垮。
    /// </summary>
    public interface IRabbitMQConnection
    {
        /// <summary>当前是否已连通 Broker</summary>
        bool IsConnected { get; }

        /// <summary>
        /// 尝试创建（并拓扑声明）一个 Channel。不可用时返回 null，绝不抛异常。
        /// </summary>
        IModel? TryCreateModel();
    }

    public class RabbitMQConnection : IRabbitMQConnection, IDisposable
    {
        private readonly RabbitMQOptions _options;
        private readonly RabbitMQTopology _topology;
        private readonly ILogger<RabbitMQConnection> _logger;
        private readonly object _gate = new();
        private IConnection? _connection;
        private bool _disposed;

        public RabbitMQConnection(
            IOptions<RabbitMQOptions> options,
            RabbitMQTopology topology,
            ILogger<RabbitMQConnection> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _topology = topology ?? throw new ArgumentNullException(nameof(topology));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsConnected
        {
            get { lock (_gate) { return _connection != null && _connection.IsOpen; } }
        }

        /// <summary>
        /// 尝试创建 Channel：未连接时先懒连接；连接/拓扑声明失败返回 null（降级信号）。
        /// </summary>
        public IModel? TryCreateModel()
        {
            if (_disposed) return null;
            if (!_options.Enabled) return null;

            try
            {
                if (!IsConnected) EnsureConnection();
                var connection = _connection;
                if (connection == null || !connection.IsOpen) return null;

                var channel = connection.CreateModel();
                // 每次拿 Channel 都做一次幂等拓扑声明：Broker 重启后队列/绑定丢失可自愈
                _topology.EnsureTopology(channel);
                return channel;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "RabbitMQ 连接不可用（{Host}:{Port}/{Vhost}），本次操作走进程内降级链路",
                    _options.HostName, _options.Port, _options.VirtualHost);
                return null;
            }
        }

        private void EnsureConnection()
        {
            lock (_gate)
            {
                if (_connection is { IsOpen: true }) return;

                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    // 6.x：AsyncEventingBasicConsumer 必须开启异步分发
                    DispatchConsumersAsync = true,
                    // 不开启连接自动恢复（AutomaticRecoveryEnabled）：由消费者宿主的循环负责重连（统一走 EnsureConnection，
                    // 避免「旧连接自我恢复 + 新连接」并存导致消费者注册到已废弃的连接上）
                    AutomaticRecoveryEnabled = false,
                    RequestedHeartbeat = TimeSpan.FromSeconds(30),
                    ClientProvidedName = "community-api"
                };

                var connection = factory.CreateConnection();
                connection.ConnectionShutdown += OnConnectionShutdown;
                _connection = connection;

                _logger.LogInformation(
                    "RabbitMQ 连接成功：{Host}:{Port} vhost={Vhost} exchange={Exchange}",
                    _options.HostName, _options.Port, _options.VirtualHost, _options.Exchange);
            }
        }

        private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("RabbitMQ 连接断开：{Reason}（将自动重连，期间走进程内降级链路）", e.ReplyText);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_gate)
            {
                try
                {
                    if (_connection != null)
                    {
                        _connection.ConnectionShutdown -= OnConnectionShutdown;
                        _connection.Dispose();
                        _connection = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "RabbitMQ 连接释放异常（忽略）");
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
