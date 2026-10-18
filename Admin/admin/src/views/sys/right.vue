<template>
    <div class="l-container">
        <div style="display:flex;gap:12px;">
            <el-card shadow="never" header="部门角色" style="width:320px" body-style="padding:8px">
                <el-table :data="dept_roles" highlight-current-row @current-change="on_dr" height="calc(100vh - 240px)" size="small">
                    <el-table-column label="部门">
                        <template #default="s">{{ dept_name(s.row.departmentId) }}</template>
                    </el-table-column>
                    <el-table-column label="角色">
                        <template #default="s">{{ role_name(s.row.roleId) }}</template>
                    </el-table-column>
                </el-table>
            </el-card>
            <el-card shadow="never" :header="`菜单授权 - ${curr_dr ? dept_name(curr_dr.departmentId) + ' / ' + role_name(curr_dr.roleId) : '请选择左侧部门角色'}`" style="flex:1">
                <el-tree ref="menu_ref" :data="menus" :props="{ label: 'name', children: 'children' }" node-key="id" show-checkbox default-expand-all @check="on_tree_check" />
                <div style="margin-top:8px">
                    <el-button type="primary" :disabled="!curr_dr" @click="confirm">确认授权</el-button>
                    <el-button @click="(menu_ref as any).setCheckedKeys([])">清空</el-button>
                </div>
            </el-card>
        </div>
    </div>
</template>

<script setup lang="ts">
import { get_depts, get_roles, bind_dept_role_menus, get_dept_role_menus, get_dept_roles } from '@/api/sys'
import { get_menu_tree } from '@/api/org'
import { Api_Code } from '@/dto/response/base'
import { close_loading, open_loading, show_notice } from '@/utils/message'
import { nextTick, onMounted, ref } from 'vue'

const dept_roles = ref<any[]>([])
const depts = ref<any[]>([])
const roles = ref<any[]>([])
const menus = ref<any[]>([])
const curr_dr = ref<any>(null)
const menu_ref = ref<any>(null)

const dept_name = (id: string) => depts.value.find(d => d.id === id)?.deptName || id
const role_name = (id: string) => roles.value.find(r => r.id === id)?.roleName || id

const bind = async () => {
    open_loading()
    const [dr, d, r, m] = await Promise.all([get_dept_roles(), get_depts(), get_roles(), get_menu_tree()])
    close_loading()
    if (dr.code === Api_Code.ok) dept_roles.value = dr.data
    if (d.code === Api_Code.ok) depts.value = d.data
    if (r.code === Api_Code.ok) roles.value = r.data
    if (m.code === Api_Code.ok) menus.value = m.data
}

const on_dr = async (row: any) => {
    curr_dr.value = row
    const tree = menu_ref.value as any
    if (!row) { tree?.setCheckedKeys([]); return }
    const res = await get_dept_role_menus(row.id)
    if (res.code === Api_Code.ok) {
        // 只传叶子节点 id：el-tree 同时接收"父节点 + 其子节点"的混合 ids 时，
        // 渲染会出现"父节点显示勾选 + 子节点没勾"的错乱（element-plus 已知 bug）。
        // 过滤掉父节点 id 后，el-tree 会按子节点状态自动算出父节点的 checked / indeterminate，
        // 视觉和数据就完全同步了。
        const leafIds = (res.data || []).filter((id: string) => is_leaf_id(id))
        nextTick(() => tree?.setCheckedKeys(leafIds))
    }
}

const confirm = async () => {
    if (!curr_dr.value) return
    const checked = (menu_ref.value as any).getCheckedKeys()
    // 保存时也只写叶子节点，丢弃任何因联动产生的父节点 id
    const menu_ids = checked.filter((id: string) => is_leaf_id(id))
    open_loading()
    const res = await bind_dept_role_menus({ deptrole_ids: [curr_dr.value.id], menu_ids })
    close_loading()
    show_notice(res)
}

// 在菜单树里递归查找一个 id，判断它是否是"叶子节点"（没有 children 字段）
const is_leaf_id = (id: string): boolean => {
    const walk = (nodes: any[]): boolean => {
        for (const n of nodes) {
            if (n.id === id) return !n.children?.length
            if (n.children?.length && walk(n.children)) return true
        }
        return false
    }
    return walk(menus.value)
}

// el-tree 的总选框在子节点非全选时显示 indeterminate（半选）。
// element-plus 默认行为是：点 indeterminate 的 checkbox 不会立即把其他子节点补上，
// 看起来就像"点了没反应"。这里手动补一次 toggle：
//   - 当前是半选或全不选 → 强制勾选所有子节点
//   - 当前已全选        → 全部取消
const on_tree_check = (node: any) => {
    if (!node?.children?.length) return
    const tree = menu_ref.value as any
    if (!tree) return
    nextTick(() => {
        const childIds: string[] = node.children.map((c: any) => c.id)
        const checked: string[] = tree.getCheckedKeys() || []
        const allChecked = childIds.length > 0 && childIds.every(id => checked.includes(id))
        // 全选 → 全部取消；其它情况 → 全部勾上（覆盖半选、全不选）
        tree.setChecked(node.id, !allChecked, false)
    })
}

onMounted(bind)
</script>

<style scoped></style>
