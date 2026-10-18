<template>
    <div>
        <el-button-group direction="vertical" class="mb-4">
            <el-button @click="edit"><i class="iconfont icon-bianji1"></i> 编辑</el-button>
            <el-button @click="add"><i class="iconfont icon-add"></i> 新增</el-button>

            
            <el-button @click="del"><i class="iconfont icon-shanchu"></i> 删除</el-button>

        </el-button-group>
    </div>
</template>

<script setup>
const emits = defineEmits(['my_del']);

const edit = () => {

}

const add = () => {

}

const del = () => {
    emits('my_del');
}


</script>

<style lang="scss" scoped></style>