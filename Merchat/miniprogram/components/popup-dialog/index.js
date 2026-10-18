"use strict";
// 通用弹窗组件 popup-dialog
// 支持两种模式：
//   mode="center"（默认）：居中弹窗
//   mode="bottom"：底部滑出面板（Bottom Sheet）
//
// Bottom Sheet 模式额外属性：
//   totalText: 底部操作栏上方显示的总金额文本
Component({
    options: { multipleSlots: true },
    properties: {
        visible: { type: Boolean, value: false },
        title: { type: String, value: '提示' },
        subtitle: { type: String, value: '' },
        /** 主操作按钮文案 */
        confirmText: { type: String, value: '确认' },
        /** 次要按钮文案，留空隐藏 */
        cancelText: { type: String, value: '取消' },
        /** 主按钮 loading 态（提交中） */
        loading: { type: Boolean, value: false },
        /** 主按钮是否禁用（loading 时自动禁用） */
        disabled: { type: Boolean, value: false },
        /** 是否显示关闭右上角 ❌ */
        showClose: { type: Boolean, value: false },
        /** 关闭遮罩（点击灰色背景）是否关闭 */
        maskClosable: { type: Boolean, value: true },
        /** 弹窗模式：center（居中弹窗）| bottom（底部面板） */
        mode: { type: String, value: 'center' },
        /** 底部模式：操作栏上方显示的金额文本（如 "应付金额：¥20.93"） */
        totalText: { type: String, value: '' },
    },
    data: {
        mounted: false,
    },
    observers: {
        visible(v) {
            if (v) {
                this.setData({ mounted: true });
            }
            else {
                setTimeout(() => this.setData({ mounted: false }), 300);
            }
        },
    },
    methods: {
        stopProp() { },
        onMaskTap() {
            if (this.properties.maskClosable && !this.properties.loading) {
                this.onClose();
            }
        },
        onClose() {
            if (this.properties.loading)
                return;
            this.triggerEvent('close');
        },
        onConfirmTap() {
            if (this.properties.loading || this.properties.disabled)
                return;
            this.triggerEvent('confirm');
        },
        onCancelTap() {
            if (this.properties.loading)
                return;
            this.triggerEvent('cancel');
        },
    },
});
