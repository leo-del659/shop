<!-- views/NotFound.vue -->
<template>
    <div class="not-found">
        <h1>404</h1>
        <p>页面走丢了</p>
        <button @click="$router.push('/')">返回首页</button>
    </div>
</template>

<script setup>
// 无需额外逻辑
</script>

<style scoped>
.not-found {
    width: 100vw;
    height: 100vh;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background: #f5f7fa;
}

h1 {
    font-size: 120px;
    color: #409eff;
    margin: 0;
}

p {
    font-size: 20px;
    color: #666;
    margin: 10px 0 30px;
}

button {
    padding: 10px 26px;
    background: #409eff;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 16px;
    cursor: pointer;
    transition: background 0.3s;
}

button:hover {
    background: #66b1ff;
}
</style>