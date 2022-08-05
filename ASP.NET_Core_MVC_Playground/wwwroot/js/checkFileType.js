$(document).ready(function () {
    $("#Image").change(function () {
        var file = $("#Image")[0].files[0];
        if (file != null) {
            if ((file.type).split("/")[0] != "image") {
                bootbox.alert({
                    message: "Please upload an image!",
                    backdrop: true
                });
                $("#Image").val('');
            };
        };
    });
});
    

