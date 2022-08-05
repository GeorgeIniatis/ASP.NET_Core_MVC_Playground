Dropzone.options.imageDropzone = { // camelized version of the `id`
    paramName: "ImageFile", // The name that will be used to transfer the file
    maxFilesize: 1, // MB
    dictDefaultMessage: "",
    maxFiles: 1,
    autoProcessQueue: false,
    addRemoveLinks: true,
    url: "null",
    createImageThumbnails: false,
    acceptedFiles: 'image/*',
    accept: function (file) {
        let fileReader = new FileReader();
        fileReader.readAsDataURL(file);
        fileReader.onloadend = function () {
            let content = fileReader.result;
            $('#ImageFile').val(content);
            file.previewElement.classList.add("dz-success");
        }
        file.previewElement.classList.add("dz-complete");
    },
    init: function () {
        this.on("removedfile", file => {
            $('#ImageFile').val('');
        });
        this.on("error", file => {
            bootbox.alert({
                message: "Please upload a smaller image!",
                backdrop: true
            });
            $('#ImageFile').val('');
            this.removeFile(file);
        });
    },
};


