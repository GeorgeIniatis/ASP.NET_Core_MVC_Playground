Dropzone.options.txtDropzone = { // camelized version of the `id`
    paramName: "TextFile", // The name that will be used to transfer the file
    maxFilesize: 1, // MB
    dictDefaultMessage: "",
    maxFiles: 1,
    autoProcessQueue: false,
    addRemoveLinks: true,
    url: "null",
    createImageThumbnails: false,
    acceptedFiles: 'text/*',
    accept: function (file) {
        let fileReader = new FileReader();
        fileReader.readAsDataURL(file);
        fileReader.onloadend = function () {
            let content = fileReader.result;
            $('#TextFile').val(content);
            file.previewElement.classList.add("dz-success");
        }
        file.previewElement.classList.add("dz-complete");
    },
    init: function () {
        this.on("removedfile", file => {
            $('#TextFile').val('');
        });
        this.on("error", file => {
            bootbox.alert({
                message: "Please upload a smaller description!",
                backdrop: true
            });
            $('#TextFile').val('');
            this.removeFile(file);
        });
    },
};