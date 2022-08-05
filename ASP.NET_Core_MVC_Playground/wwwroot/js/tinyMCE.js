function tinyTextArea(text) {
    tinymce.init({
        selector: 'textarea#TinyTextArea',
        plugins: 'image autolink lists media table',
        toolbar: 'image table tableofcontents',
        toolbar_mode: 'floating',
        onchange_callback: "loadContent",
        setup: function (editor) {
            editor.on("init", function () {
                this.setContent(text);
            });
            editor.on("change", function () {
                $("#TextArea").val(this.getContent());
            });
        },
    });
};