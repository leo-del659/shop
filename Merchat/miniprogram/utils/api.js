"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.emojiFor = emojiFor;
exports.resolveCoverImage = resolveCoverImage;
exports.getHome = getHome;
exports.getAllOrders = getAllOrders;
exports.getGoodsList = getGoodsList;
exports.getGoodsDetail = getGoodsDetail;
exports.toggleGoodsStatus = toggleGoodsStatus;
exports.restockGoods = restockGoods;
exports.updateGoods = updateGoods;
exports.getGoodsSpecs = getGoodsSpecs;
exports.addGoods = addGoods;
exports.getOrderList = getOrderList;
exports.getBillList = getBillList;
exports.getBillDetail = getBillDetail;
exports.getOrderDetail = getOrderDetail;
exports.orderAction = orderAction;
exports.getBalance = getBalance;
exports.getReconcile = getReconcile;
exports.getWithdrawals = getWithdrawals;
exports.submitWithdraw = submitWithdraw;
exports.adaptMerchant = adaptMerchant;
exports.getProfile = getProfile;
exports.updateProfile = updateProfile;
exports.changePassword = changePassword;
exports.getBank = getBank;
exports.updateBank = updateBank;
exports.getQualifications = getQualifications;
exports.addQualification = addQualification;
exports.getAddressList = getAddressList;
exports.getAddress = getAddress;
exports.saveAddress = saveAddress;
exports.removeAddress = removeAddress;
exports.setDefaultAddress = setDefaultAddress;
exports.getChatConversations = getChatConversations;
exports.getChatMessages = getChatMessages;
exports.sendChatMessage = sendChatMessage;
exports.getMerchantNotices = getMerchantNotices;
exports.readMerchantNotices = readMerchantNotices;
exports.getUnreadCount = getUnreadCount;
const config_1 = require("./config");
const request_1 = require("./request");
const util_1 = require("./util");
// 商户端接口封装。统一走 .NET 后端（Community.Api.Admin）：
// REST 路径 /api/merchant/*，响应 { code, message, data }，需 JWT Bearer。
function emojiFor(name) {
    const map = [
        [/苹果|梨|桃|橘|橙|莓|芒果|火龙果|葡萄|水果|奇异果|车厘子|蜜柚|李子/, '🍎'],
        [/菜|白菜|青菜|生菜|菠菜|番茄|西红柿|黄瓜|冬瓜|秋葵|芽|苗|南瓜|辣椒/, '🥬'],
        [/米|面|粉|粮|油|酱油|醋|调料|香料|花椒|八角|豆瓣|糖/, '🌾'],
        [/鸡|鸭|肉|猪|牛|羊|蛋|肠/, '🍗'],
        [/鱼|虾|蟹|贝|泥鳅|海鲜|三文鱼/, '🐟'],
        [/牛奶|酸奶|奶酪|乳/, '🥛'],
        [/水|饮料|可乐|茶|酒|啤酒|果汁|咖啡/, '🥤'],
        [/饼干|巧克力|坚果|零食|曲奇|瓜子|薯片/, '🍪'],
        [/保鲜|袋|盒|纸|手套|清洁|洗洁|餐盒|托盘|桌布/, '🧴'],
        [/菌|菇|木耳|笋|干货/, '🍄'],
    ];
    for (const [re, emoji] of map)
        if (re.test(name || ''))
            return emoji;
    return '📦';
}
function mapGoodsStatus(isSale, auditStatus) {
    if (auditStatus === 0)
        return 'draft';
    return isSale === 1 ? 'published' : 'disabled';
}
function mapOrderStatus(orderStatus) {
    if (orderStatus === 2 || orderStatus === 3)
        return 'shipping';
    if (orderStatus === 4 || orderStatus === 6)
        return 'completed';
    if (orderStatus === 9)
        return 'cancelled';
    return 'pending';
}
function mapPayStatus(payStatus) {
    if (payStatus === 1)
        return 'paid';
    if (payStatus === 2)
        return 'partial';
    return 'unpaid';
}
// 本地兜底图文件名集合（已打包进小程序，走相对路径即可）
const LOCAL_FALLBACK_IMAGES = new Set(Array.from({ length: 17 }, (_, i) => `goods_${i.toString().padStart(2, '0')}.webp`));
function isLocalFallbackImage(path) {
    const name = path.split('/').pop() || '';
    return LOCAL_FALLBACK_IMAGES.has(name);
}
// 管理后台(.NET)上传的文件（/api/file/xxx）统一经 .NET 后端 6001 提供访问
function toFullImageUrl(coverImg) {
    if (!coverImg)
        return '';
    if (coverImg.startsWith('http'))
        return coverImg;
    let path = coverImg.startsWith('/') ? coverImg : '/' + coverImg;
    const sep = path.includes('?') ? '&' : '?';
    // 商户端 / 后台 .NET 上传图（/api/file/xxx）：统一走 /api/file/img?name= 可靠下载端点，
    // 与商户端 Chat 的 resolveUrl 行为完全一致（直接 /api/file/{name} 会因 MVC 路由对文件名中的点截断而不可靠）
    if (path.startsWith('/api/file/')) {
        return (0, request_1.resolveUrl)(path);
    }
    // 小程序内置兜底图（goods_xx.webp）直接走相对路径
    if ((path.startsWith('/images/') || path.startsWith('/uploads/')) &&
        isLocalFallbackImage(path)) {
        return `${path}${sep}t=${Date.now()}`;
    }
    return `${config_1.API_BASE}${path}${sep}t=${Date.now()}`;
}
// 当前 .NET uploads 目录为空，/api/file/{id} 全部 404。
// 用商品名/ID 做确定性哈希，从本地 17 张产品图中选一张兜底展示。
function deterministicProductImage(seed, orderIndex) {
    const images = [
        '/images/products/goods_00.webp',
        '/images/products/goods_01.webp',
        '/images/products/goods_02.webp',
        '/images/products/goods_03.webp',
        '/images/products/goods_04.webp',
        '/images/products/goods_05.webp',
        '/images/products/goods_06.webp',
        '/images/products/goods_07.webp',
        '/images/products/goods_08.webp',
        '/images/products/goods_09.webp',
        '/images/products/goods_10.webp',
        '/images/products/goods_11.webp',
        '/images/products/goods_12.webp',
        '/images/products/goods_13.webp',
        '/images/products/goods_14.webp',
        '/images/products/goods_15.webp',
        '/images/products/goods_16.webp',
    ];
    // 指定顺序索引时，按图片编号依次分配（00,01,02…），保证列表图片规律排列
    if (orderIndex !== undefined && orderIndex >= 0) {
        return images[orderIndex % images.length];
    }
    let hash = 0;
    for (let i = 0; i < seed.length; i++) {
        hash = (hash << 5) - hash + seed.charCodeAt(i);
        hash |= 0;
    }
    const idx = Math.abs(hash) % images.length;
    return images[idx];
}
function resolveCoverImage(rawCover, name, id, orderIndex) {
    if (!rawCover)
        return deterministicProductImage(String(id || name), orderIndex);
    const normalized = rawCover.trim();
    // 本地兜底图（goods_xx.webp）：直接使用相对路径，小程序包内可访问
    if (normalized.startsWith('/images/') && isLocalFallbackImage(normalized)) {
        return normalized;
    }
    // 旧 Python 后端上传的 UUID 图片（/images/products/{uuid}.webp）或 /uploads/ 路径：
    // .NET 后端不提供这些静态文件服务，请求会 404 → 统一回退到按顺序分配的本地产品图
    if (normalized.startsWith('/images/') ||
        normalized.startsWith('/uploads/')) {
        return deterministicProductImage(String(id || name), orderIndex);
    }
    // 管理后台 .NET 上传图（/api/file/{id}）与完整外链：走 .NET 后端 6001
    if (normalized.startsWith('/api/file/') ||
        /^https?:\/\//i.test(normalized)) {
        return toFullImageUrl(normalized);
    }
    return deterministicProductImage(String(id || name), orderIndex);
}
function adaptGoods(raw, orderIndex) {
    const name = raw.goodsName || raw.name || '商品';
    const id = raw.id ?? '';
    const rawCover = raw.coverImg || raw.cover_img || '';
    const rawImgList = raw.imgList || raw.img_list || '';
    const imgList = Array.isArray(rawImgList)
        ? rawImgList
        : String(rawImgList).split(',').filter(Boolean);
    const fallbackEmoji = emojiFor(name);
    const coverImg = resolveCoverImage(rawCover, name, id, orderIndex);
    const allImages = [
        ...(coverImg ? [coverImg] : []),
        ...imgList
            .filter((u) => u !== rawCover)
            .map((u) => resolveCoverImage(u, name, id))
            .filter(Boolean),
    ];
    return {
        id,
        name,
        category: raw.category || '-',
        price: Number(raw.price ?? raw.salePrice ?? 0),
        stock: Number(raw.stock ?? 0),
        unit: raw.unit || '件',
        status: mapGoodsStatus(raw.isSale, raw.auditStatus),
        emoji: fallbackEmoji,
        coverImg,
        sales: Number(raw.sales ?? 0),
        marketPrice: Number(raw.salePrice ?? raw.price ?? 0),
        costPrice: Number(raw.supplyPrice ?? 0),
        merchant: raw.merchantName || '',
        safeStock: 50,
        alertStock: 20,
        launchTime: raw.createTime || '',
        description: raw.description || raw.goodsDesc || raw.detail || '',
        images: allImages.length
            ? allImages.map((url, idx) => ({
                id: idx,
                url,
                emoji: fallbackEmoji,
                label: name,
            }))
            : [{ id: 0, emoji: fallbackEmoji, label: name }],
        skus: Array.isArray(raw.skus)
            ? raw.skus.map((s) => ({
                id: s.id != null ? String(s.id) : '',
                specId: s.specId != null ? String(s.specId) : undefined,
                skuName: s.skuName || s.sku_name || '',
                skuCode: s.skuCode || s.sku_code || '',
                price: Number(s.price ?? 0),
                stock: Number(s.stock ?? 0),
            }))
            : undefined,
        specs: Array.isArray(raw.specs)
            ? raw.specs.map((s) => ({
                id: s.id != null ? String(s.id) : '',
                specName: s.specName || s.spec_name || '',
                sort: Number(s.sort ?? 0),
            }))
            : undefined,
        skuCount: Number(raw.skuCount ?? (Array.isArray(raw.skus) ? raw.skus.length : 0)),
        priceMin: Number(raw.priceMin ?? raw.price ?? 0),
        priceMax: Number(raw.priceMax ?? raw.salePrice ?? raw.price ?? 0),
    };
}
function adaptOrderItem(raw) {
    return {
        name: raw.goodsName || raw.goods_name || raw.name || '-',
        spec: raw.specName || raw.spec_name || raw.spec || '-',
        qty: Number(raw.buyNum ?? raw.buy_num ?? raw.qty ?? 1),
        price: Number(raw.buyPrice ?? raw.buy_price ?? raw.price ?? 0),
        img: raw.goodsImg || raw.goods_img || '',
    };
}
function adaptOrder(raw) {
    const status = mapOrderStatus(raw.orderStatus);
    const payStatus = mapPayStatus(raw.payStatus);
    const items = (raw.items || []).map(adaptOrderItem);
    // 订单状态权威文案：后端返回 statusText/statusCls 时优先采用，否则按 order_status 数值本地映射，
    // 确保前后端展示一致（不再用 pay_status 充当订单状态）。
    const detail = raw.statusText
        ? { label: String(raw.statusText), cls: String(raw.statusCls || '') }
        : (0, util_1.orderStatusDetail)(Number(raw.orderStatus ?? raw.order_status ?? 0));
    return {
        id: String(raw.id || ''),
        orderNo: raw.orderNo || raw.order_no || String(raw.id || ''),
        merchant: raw.merchantName || raw.merchant || '-',
        merchantName: raw.merchantName || '',
        merchantContact: raw.merchantContact || '',
        merchantPhone: raw.merchantPhone || '',
        merchantAddress: raw.merchantAddress || '',
        merchantId: raw.merchantId || '',
        clientId: raw.clientId || '',
        status,
        rawStatus: Number(raw.orderStatus ?? raw.order_status ?? 0),
        payStatus,
        detailStatusLabel: detail.label,
        detailStatusCls: detail.cls,
        time: raw.createTime || raw.time || '',
        payTime: raw.payTime || '',
        receiver: [raw.receiverName, raw.receiverMobile, raw.address]
            .filter(Boolean)
            .join(' '),
        remark: raw.remark || '',
        rejectReason: raw.rejectReason || raw.reject_reason || '',
        items: items.length ? items : [{ name: '-', qty: 1, price: 0 }],
        total: Number(raw.totalAmount ?? raw.total ?? 0),
        goodsSummary: raw.goodsSummary || items.map((it) => it.name).filter(Boolean).join('、') || '-',
        totalQty: items.reduce((sum, it) => sum + (it.qty || 0), 0) || Number(raw.quantity ?? raw.buyNum ?? 0),
        logistics: buildLogistics(Number(raw.orderStatus ?? raw.order_status ?? 0)),
    };
}
function buildLogistics(rawStatus) {
    const now = new Date().toLocaleString('zh-CN');
    if (rawStatus === 4 || rawStatus === 6) {
        return [
            { text: '订单已完成', time: now, active: true, current: true },
            { text: '商品已发货', time: now, active: true },
            { text: '商家已接单', time: now, active: true },
            { text: '订单已提交', time: now, active: true },
        ];
    }
    if (rawStatus === 9) {
        return [
            { text: '订单已取消', time: now, active: false, current: true },
            { text: '订单已提交', time: now, active: true },
        ];
    }
    if (rawStatus === 3) {
        return [
            { text: '商品已发货', time: now, active: true, current: true },
            { text: '商家已接单', time: now, active: true },
            { text: '订单已提交', time: now, active: true },
        ];
    }
    if (rawStatus === 2) {
        return [
            { text: '商家已接单', time: now, active: true, current: true },
            { text: '订单已提交', time: now, active: true },
        ];
    }
    return [
        { text: '等待商家接单', time: now, active: true, current: true },
        { text: '订单已提交', time: now, active: true },
    ];
}
// 按创建时间倒序排列，保证新增商品置顶（后端已按 create_time DESC 返回，此处兜底）
function sortGoodsByTime(rows) {
    return rows.slice().sort((a, b) => {
        const ta = new Date(a.createTime || a.create_time || 0).getTime();
        const tb = new Date(b.createTime || b.create_time || 0).getTime();
        return tb - ta;
    });
}
// ---------- 首页 ----------
async function getHome() {
    const raw = await (0, request_1.request)({ path: '/api/merchant/home', method: 'GET' });
    return {
        merchant: adaptMerchant(raw.merchant || {}),
        kpi: raw.kpi || { orders: 0, revenue: 0, products: 0, pending: 0 },
        // 透传后端 today / notices：此前仅取部分字段，首页今日数据与公告实际取不到
        today: raw.today || undefined,
        notices: (raw.notices || []).map((n) => ({
            id: n.id,
            title: n.title || '',
            content: n.content || '',
            type: n.type || 1,
            typeText: n.typeText || '',
            createTime: n.createTime || '',
        })),
        products: sortGoodsByTime(raw.products || []).map((raw, i) => adaptGoods(raw, i)),
        todos: (raw.todos || []).map((t) => ({
            type: t.type || 'blue',
            text: t.text || '',
            time: t.time || '',
        })),
    };
}
// ---------- 订单（用于趋势计算）----------
async function getAllOrders() {
    const rows = await (0, request_1.request)({ path: '/api/merchant/orders?status=all&limit=100', method: 'GET' });
    return (rows || []).map((r) => ({
        id: r.id || '',
        totalAmount: Number(r.totalAmount ?? r.total ?? 0),
        orderStatus: Number(r.orderStatus ?? r.order_status ?? 0),
        payStatus: Number(r.payStatus ?? 0),
        createTime: r.createTime || r.time || '',
    }));
}
async function getGoodsList(filter = 'all', opts) {
    const map = {
        all: 'all',
        published: 'on',
        draft: 'audit',
        disabled: 'off',
        'low-stock': 'low',
    };
    const data = { filter: map[filter] };
    if (opts?.keyword)
        data.keyword = opts.keyword;
    if (opts?.category)
        data.category = opts.category;
    const rows = await (0, request_1.request)({ path: '/api/merchant/goods', method: 'GET', data });
    // 后端 get_goods 已按 create_time DESC 返回，新增商品自然置顶
    return (rows || []).map((raw, i) => adaptGoods(raw, i));
}
async function getGoodsDetail(id, orderIndex) {
    const raw = await (0, request_1.request)({ path: '/api/merchant/goods/' + id, method: 'GET' });
    return raw ? adaptGoods(raw, orderIndex) : undefined;
}
async function toggleGoodsStatus(id) {
    const data = await (0, request_1.request)({ path: '/api/merchant/goods/' + id + '/toggle', method: 'POST' });
    return mapGoodsStatus(data.isSale, data.auditStatus);
}
async function restockGoods(id, qty) {
    const data = await (0, request_1.request)({ path: '/api/merchant/goods/' + id + '/restock', method: 'POST', data: { qty } });
    return adaptGoods(data);
}
async function updateGoods(id, data) {
    const raw = await (0, request_1.request)({ path: '/api/merchant/goods/' + id, method: 'POST', data: data });
    return adaptGoods(raw);
}
/** 查询商品规格（编辑多规格商品时回填） */
async function getGoodsSpecs(id) {
    const raw = await (0, request_1.request)({ path: '/api/merchant/goods/' + id + '/specs', method: 'GET' });
    return { specs: (raw && raw.specs) || [], skus: (raw && raw.skus) || [] };
}
async function addGoods(data) {
    const raw = await (0, request_1.request)({ path: '/api/merchant/goods', method: 'POST', data: data });
    return adaptGoods(raw);
}
async function getOrderList(status = 'all') {
    const map = {
        all: 'all',
        pending: 'pending',
        shipping: 'shipping',
        completed: 'completed',
        cancelled: 'cancelled',
    };
    const rows = await (0, request_1.request)({ path: '/api/merchant/orders', method: 'GET', data: { status: map[status] } });
    return (rows || []).map(adaptOrder);
}
async function getBillList(status = 'all') {
    const map = {
        all: 'all',
        pending: 'pending',
        shipping: 'shipping',
        completed: 'completed',
        cancelled: 'cancelled',
    };
    const rows = await (0, request_1.request)({ path: '/api/merchant/bills', method: 'GET', data: { status: map[status] } });
    // 兼容 .NET 默认 PascalCase / snake_case / camelCase 三种字段格式
    const pick = (obj, ...keys) => {
        for (const k of keys) {
            if (obj[k] != null)
                return obj[k];
        }
        return undefined;
    };
    return (rows || []).map((r, i) => {
        const id = String(pick(r, 'id') || '');
        const name = pick(r, 'goodsName', 'GoodsName', 'goods_name') || '-';
        const rawCover = pick(r, 'coverImg', 'cover_img', 'CoverImg') || '';
        return {
            id,
            billNo: pick(r, 'billNo', 'billno', 'BillNo', 'Billno') || id,
            merchantId: pick(r, 'merchantId', 'MerchantId') || '',
            merchantName: pick(r, 'merchantName', 'MerchantName') || '-',
            goodsName: name,
            coverImg: resolveCoverImage(rawCover, name, id, i),
            hasImage: !!String(rawCover).trim(),
            emoji: emojiFor(name),
            buyNum: Number(pick(r, 'buyNum', 'BuyNum', 'buy_num') ?? 0),
            amount: Number(pick(r, 'amount', 'Amount') ?? 0),
            orderStatus: Number(pick(r, 'orderStatus', 'OrderStatus') ?? 0),
            statusLabel: pick(r, 'statusText', 'StatusText', 'statusLabel') || '-',
            statusCls: pick(r, 'statusCls', 'StatusCls') || '',
            createTime: pick(r, 'createTime', 'CreateTime', 'create_time') || '',
        };
    });
}
/** 账单详情（含物流时间线） */
async function getBillDetail(id) {
    return (0, request_1.request)({ path: '/api/merchant/bills/' + id, method: 'GET' });
}
async function getOrderDetail(id) {
    const raw = await (0, request_1.request)({ path: '/api/merchant/orders/' + id, method: 'GET' });
    return raw ? adaptOrder(raw) : undefined;
}
async function orderAction(id, action, reason) {
    const actionMap = {
        accept: 'accept',
        ship: 'ship',
        complete: 'complete',
        reject: 'reject',
        cancel: 'reject',
    };
    const raw = await (0, request_1.request)({
        path: '/api/merchant/orders/' + id + '/action',
        method: 'POST',
        data: { action: actionMap[action] || action, reason },
    });
    return raw ? adaptOrder(raw) : undefined;
}
function roundMoney(n) {
    return Math.round((n + Number.EPSILON) * 100) / 100;
}
async function getBalance() {
    const data = await (0, request_1.request)({ path: '/api/merchant/balance', method: 'GET' });
    return {
        totalPaid: roundMoney(Number(data.totalPaid) || 0),
        withdrawn: roundMoney(Number(data.withdrawn) || 0),
        available: roundMoney(Number(data.available) || 0),
        paidOrderCount: Number(data.paidOrderCount) || 0,
    };
}
async function getReconcile(monthKey) {
    const data = await (0, request_1.request)({ path: '/api/merchant/reconcile', method: 'GET', data: { month: monthKey } });
    const os = (data.orderSummary || data.summary);
    const orders = (data.bills || []).map((b) => ({
        id: String(b.id || b.billNo || ''),
        merchant: b.goodsName || '-',
        status: b.orderStatus === 3 ? 'completed' : b.payStatus === 1 ? 'completed' : 'pending',
        payStatus: b.payStatus === 1 ? 'paid' : b.payStatus === 2 ? 'partial' : 'unpaid',
        time: b.createTime || '',
        total: Number(b.amount || 0),
        items: [{ name: b.goodsName || '-', qty: Number(b.buyNum || 1), price: Number(b.amount || 0) }],
    }));
    return {
        monthKey,
        monthLabel: monthKey || '全部',
        summary: {
            revenue: os.revenue,
            orders: os.count,
            paid: os.paid,
        },
        orders,
        payments: [],
        withdrawals: [],
    };
}
function maskBankAccount(account) {
    if (!account)
        return '';
    if (account.length <= 4)
        return account;
    return '尾号' + account.slice(-4);
}
async function getWithdrawals() {
    const rows = await (0, request_1.request)({ path: '/api/merchant/withdrawals', method: 'GET' });
    // 后端状态：1=待处理 2=已��款 其他=其他
    const mapStatus = (s) => s === 2 ? 'success' : s === 1 ? 'pending' : 'failed';
    return (rows || []).map((r) => ({
        id: r.id,
        amount: Number(r.amount || 0),
        status: mapStatus(Number(r.status ?? 0)),
        time: r.createTime || '',
        account: [r.bankName, maskBankAccount(r.bankAccount)].filter(Boolean).join(' '),
        remark: r.remark || '',
    }));
}
async function submitWithdraw(amount, remark) {
    const data = (await (0, request_1.request)({
        path: '/api/merchant/withdraw',
        method: 'POST',
        data: { amount, remark },
    })) || {};
    // 直接提现、无需审核：后端已直接置为已打款（status=2）
    return {
        id: data.id || 0,
        amount: Number(data.amount || amount),
        status: 'success',
        time: data.createTime || new Date().toISOString(),
        account: [data.bankName, maskBankAccount(data.bankAccount)].filter(Boolean).join(' '),
        remark: remark || '',
    };
}
// ---------- 个人中心 ----------
function adaptMerchant(raw) {
    return {
        id: raw.id,
        merchantName: raw.merchantName || '',
        merchantNo: raw.merchantNo || '',
        avatar: raw.avatar || '',
        account: raw.account || '',
        contact: raw.contact || '',
        phone: raw.phone || '',
        address: raw.address || '',
        createTime: raw.createTime || '',
        isBan: raw.isBan,
        level: raw.levelName || raw.level || '',
        email: raw.email || '',
        joinDate: raw.createTime ? raw.createTime.slice(0, 10) : '',
        avatarText: (raw.merchantName || '商').charAt(0),
        settlementAccount: [raw.bankName, raw.accountName, raw.bankAccount].filter(Boolean).join(' '),
        shopStatus: raw.isBan === 1 ? '已冻结' : '营业中',
        qualificationCount: Number(raw.qualificationCount ?? raw.qualification_count ?? 0),
    };
}
async function getProfile() {
    const raw = await (0, request_1.request)({ path: '/api/merchant/profile', method: 'GET' });
    return {
        merchant: adaptMerchant(raw.merchant || {}),
        stats: raw.stats || { orders: 0, revenue: 0, products: 0, pending: 0 },
        merchants: raw.merchants || [],
    };
}
async function updateProfile(data) {
    await (0, request_1.request)({ path: '/api/merchant/profile', method: 'POST', data });
}
async function changePassword(oldPassword, newPassword) {
    await (0, request_1.request)({
        path: '/api/merchant/change-password',
        method: 'POST',
        data: { oldPassword, newPassword },
    });
}
async function getBank() {
    const rows = await (0, request_1.request)({ path: '/api/merchant/bank', method: 'GET' });
    return rows && rows[0] ? rows[0] : null;
}
async function updateBank(data) {
    await (0, request_1.request)({ path: '/api/merchant/bank', method: 'POST', data: data });
}
async function getQualifications() {
    const rows = await (0, request_1.request)({ path: '/api/merchant/qualification', method: 'GET' });
    return (rows || []).map((r) => ({ ...r, fileUrl: r.fileUrl ? toFullImageUrl(r.fileUrl) : '' }));
}
async function addQualification(data) {
    await (0, request_1.request)({ path: '/api/merchant/qualification', method: 'POST', data: data });
}
// ---------- 收货地址（复用后端 api/Address/*，按 JWT unique_name 自动隔离角色） ----------
async function getAddressList() {
    return (0, request_1.request)({ path: '/api/Address/List', method: 'GET' });
}
async function getAddress(id) {
    return (0, request_1.request)({ path: '/api/Address/Get', method: 'GET', data: { id } });
}
async function saveAddress(payload) {
    return (0, request_1.request)({ path: '/api/Address/Save', method: 'POST', data: payload });
}
async function removeAddress(id) {
    return (0, request_1.request)({ path: '/api/Address/Remove', method: 'POST', data: { id } });
}
async function setDefaultAddress(id) {
    return (0, request_1.request)({ path: '/api/Address/SetDefault', method: 'POST', data: { id } });
}
/** 会话列表：聊过的买家（含最后一条消息与未读数） */
async function getChatConversations() {
    return (0, request_1.request)({ path: '/api/merchant/chat/conversations', method: 'GET' });
}
/**
 * 拉取与某买家的消息（最近 50 条，升序返回）
 * @param after 最后一条消息的 createTime，用于增量轮询
 */
async function getChatMessages(clientId, after, pageSize = 50) {
    const data = { pageSize };
    if (after)
        data.after = after;
    return (0, request_1.request)({
        path: '/api/merchant/chat/clients/' + clientId + '/messages',
        method: 'GET',
        data,
    });
}
/** 发送消息给买家 */
async function sendChatMessage(clientId, content) {
    return (0, request_1.request)({
        path: '/api/merchant/chat/clients/' + clientId + '/messages',
        method: 'POST',
        data: { content },
    });
}
/**
 * 通知列表（进入消息中心时调用）
 * 后端会先扫描该商户在售商品，把低库存商品自动生成库存预警，再返回列表。
 */
async function getMerchantNotices() {
    return (0, request_1.request)({
        path: '/api/merchant/notices',
        method: 'GET',
    });
}
/** 全部标记已读 */
async function readMerchantNotices() {
    return (0, request_1.request)({ path: '/api/merchant/notices/read', method: 'POST' });
}
/** 未读数汇总（首页消息按钮红点）：聊天未读 + 通知未读 */
async function getUnreadCount() {
    return (0, request_1.request)({
        path: '/api/merchant/chat/unread',
        method: 'GET',
    });
}
