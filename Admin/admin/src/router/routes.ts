const routes = [
    {
        path: '/',
        component: () => import('@/views/layout.vue'),
        meta: { check: true },
        children: [
            {
                path: '',
                component: () => import('@/views/home/index.vue')
            },
            { path: 'goods/list', component: () => import('@/views/goods/list.vue') },
            { path: 'goods/audit', component: () => import('@/views/goods/audit.vue') },
            { path: 'goods/sku', component: () => import('@/views/goods/sku.vue') },
            { path: 'review/list', component: () => import('@/views/review/list.vue') },
            { path: 'qualification', component: () => import('@/views/qualification/index.vue') },
            { path: 'qualification/merchant', component: () => import('@/views/qualification/merchant.vue') },
            { path: 'merchant', component: () => import('@/views/merchant/index.vue') },
            // sys_menu「商户列表」的真实 path 是 /merchant/list，补上避免点击菜单进 404
            { path: 'merchant/list', component: () => import('@/views/merchant/index.vue') },
            { path: 'merchant/rank', component: () => import('@/views/rank/merchant.vue') },
            { path: 'settle', component: () => import('@/views/settle/index.vue') },
            { path: 'settle/booth', component: () => import('@/views/booth/index.vue') },
            { path: 'settle/boothrent', component: () => import('@/views/booth/rent.vue') },
            { path: 'settle/feeconfig', component: () => import('@/views/fee/config.vue') },
            { path: 'settle/receipt', component: () => import('@/views/receipt/index.vue') },
            { path: 'settle/merchantbill', component: () => import('@/views/settle/merchantBill.vue') },
            { path: 'order/list', component: () => import('@/views/order/list.vue') },
            { path: 'order/refund', component: () => import('@/views/order/refund.vue') },
            { path: 'order/refund/list', component: () => import('@/views/order/refund/list.vue') },
            { path: 'order/review', component: () => import('@/views/order/review.vue') },
            { path: 'logistics', component: () => import('@/views/logistics/index.vue') },
            { path: 'stock', component: () => import('@/views/stock/index.vue') },
            { path: 'sys/org', component: () => import('@/views/sys/org.vue') },
            { path: 'sys/menu', component: () => import('@/views/sys/menu.vue') },
            { path: 'sys/notice', component: () => import('@/views/sys/notice.vue') },
            { path: 'sys/right', component: () => import('@/views/sys/right.vue') },
        ]
    },
    { path: '/login', component: () => import('@/views/login/index.vue') },
    {
        path: '/:pathMatch(.*)*',
        name: 'NotFound',
        component: () => import('@/views/error/404.vue')
    },
]

export default routes
