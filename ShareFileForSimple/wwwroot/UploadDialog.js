window.initDragAndDrop = (zoneId, inputId) => {
    const dropZone = document.getElementById(zoneId);
    const fileInput = document.getElementById(inputId);
    const preview = document.getElementById('filePreview');

    if (!dropZone || !fileInput) return;

    // --- 通用处理函数：校验 + 预览 ---
    const updateFilesUI = (files) => {
        // 1. 数量限制校验
        if (files.length > 9) {
            alert("一次最多只能上传 9 个文件！");
            fileInput.value = ""; // 清空选择
            if (preview) preview.innerHTML = "";
            return false;
        }

        // 2. 显示预览
        if (preview) {
            if (files.length > 0) {
                const fileNames = Array.from(files).map(f => f.name).join(', ');
                preview.innerHTML = `<small style="color:var(--mud-palette-primary)">已就绪 (${files.length}): ${fileNames}</small>`;
            } else {
                preview.innerHTML = "";
            }
        }
        return true;
    };

    // --- A. 处理点击浏览的选择 ---
    fileInput.addEventListener('change', () => {
        updateFilesUI(fileInput.files);
    });

    // --- B. 处理拖拽逻辑 ---
    const preventDefaults = (e) => {
        e.preventDefault();
        e.stopPropagation();
    };

    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
    });

    ['dragenter', 'dragover'].forEach(eventName => {
        dropZone.addEventListener(eventName, () => dropZone.classList.add('drop-zone-active'), false);
    });

    ['dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, () => dropZone.classList.remove('drop-zone-active'), false);
    });

    dropZone.addEventListener('drop', (e) => {
        const dt = e.dataTransfer;
        const droppedFiles = dt.files;

        if (droppedFiles && droppedFiles.length > 0) {
            // 执行校验
            if (updateFilesUI(droppedFiles)) {
                fileInput.files = droppedFiles; // 只有校验通过才赋值
            }
        }
    });
};
/**
 * 极简上传逻辑
 * @param {object} dotNetHelper - Blazor 传递的组件引用
 * @param {string} inputId - 页面上 input 元素的 ID
 * @param {string} description - 记录描述
 */
// 确保 startUpload 里面也有最后的防御校验
window.startUpload = (dotNetHelper, inputId, description) => {
    const fileInput = document.getElementById(inputId);
    if (!fileInput || fileInput.files.length === 0) {
        alert("请先选择文件！");
        dotNetHelper.invokeMethodAsync("UploadFinished", false);
        return;
    }

    if (fileInput.files.length > 9) {
        alert("文件数量不能超过 9 个");
        dotNetHelper.invokeMethodAsync("UploadFinished", false);
        return;
    }

    const formData = new FormData();
    formData.append("description", description);
    for (let i = 0; i < fileInput.files.length; i++) {
        formData.append("files", fileInput.files[i]);
    }

    const xhr = new XMLHttpRequest();
    xhr.open("POST", "/api/Upload/PostFiles", true);
    xhr.upload.onprogress = (e) => {
        if (e.lengthComputable) {
            const percent = Math.round((e.loaded / e.total) * 100);
            dotNetHelper.invokeMethodAsync("UpdateProgress", percent);
        }
    };
    xhr.onload = () => dotNetHelper.invokeMethodAsync("UploadFinished", xhr.status === 200);
    xhr.onerror = () => dotNetHelper.invokeMethodAsync("UploadFinished", false);
    xhr.send(formData);
};