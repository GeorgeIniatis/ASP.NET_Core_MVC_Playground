/*
 * Everything upload through Dropzone's own POST request not the form itself
 * In accept function remember done();
 * isFileUploaded is a dummy variable used to help us check if a file has been uploaded or not
 * processQueue -> sending -> complete
 */

Dropzone.options.spreadsheetDropzone = { // camelized version of the `id`
    paramName: "SpreadsheetFile", // The name that will be used to transfer the file
    maxFilesize: 5, // MB
    dictDefaultMessage: "",
    autoProcessQueue: false,
    addRemoveLinks: true,
    url: "/Spreadsheet/UploadSellers",
    createImageThumbnails: true,
    acceptedFiles: 'text/csv, application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    accept: function (file, done) {
        $('#isFileUploaded').val("true");
        file.previewElement.classList.add("dz-success");
        file.previewElement.classList.add("dz-complete");
        done();
    },
    init: function () {
        var submitButton = $("#SubmitButton");
        wrapperThis = this;

        submitButton.click(function (e) {
            e.preventDefault();
            e.stopPropagation();

            if ($('#isFileUploaded').val() == '') {
                bootbox.alert({
                    message: "Please upload a spreadsheet file!",
                    backdrop: true
                });
            }
            else
            {
                wrapperThis.processQueue();
            }
        });

        wrapperThis.on("removedfile", file => {
            $('#isFileUploaded').val('');
        });

        wrapperThis.on("error", file => {
            bootbox.alert({
                message: "Please upload a smaller spreadsheet!",
                backdrop: true
            });
            $('#isFileUploaded').val('');
            wrapperThis.removeFile(file);
        });

        wrapperThis.on("sending", function (file, xhr, formData) {
            formData.append("__RequestVerificationToken", $('input:hidden[name="__RequestVerificationToken"]').val());
            formData.append("SpreadsheetFile", file);
            formData.append("TestVariable", "JustATest");

            for (var pair of formData.entries()) {
                console.log(pair[0] + ', ' + pair[1]);
            }
        });

        wrapperThis.on("complete", file => {
            wrapperThis.removeFile(file);
            // Perform a GET request
            window.location.reload();
        });
        
    },
};


