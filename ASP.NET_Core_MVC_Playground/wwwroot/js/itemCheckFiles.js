$(document).ready(function () {
    $("#Create-Item-Form").submit(function (e) {
        if (($('#ImageFile').val() != '') && ($('#TextFile').val() != '')) {
            e.submit();
        } else {

            e.preventDefault();

            if (($('#ImageFile').val() == '') && ($('#TextFile').val() == '')) {
                bootbox.alert({
                    message: resources.UploadImageAndDescription,
                    backdrop: true
                });
                return;
            }
            if ($('#ImageFile').val() == '') {
                bootbox.alert({
                    message: resources.UploadImage,
                    backdrop: true
                });
                return;
            }
            if ($('#TextFile').val() == '') {
                bootbox.alert({
                    message: resources.UploadDescription,
                    backdrop: true
                });
                return;
            }
        }
    });
});