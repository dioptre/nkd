if (typeof jQuery == 'undefined') {
    var script = document.createElement('script');
    script.type = "text/javascript";
    script.src = "/Modules/NKD/Scripts/jquery-1.7.1.min.js";
    document.getElementsByTagName('head')[0].appendChild(script);
}

(function () {
    // Poll for jQuery to come into existance
    var checkReady = function (callback) {
        if (window.jQuery) {
            callback(jQuery);
        }
        else {
            window.setTimeout(function () { checkReady(callback); }, 100);
        }
    };

    // Start polling...
    checkReady(function ($) {
        var script = document.createElement('script');
        script.type = "text/javascript";
        script.src = "/Modules/NKD/Scripts/jquery.validate.js";
        document.getElementsByTagName('head')[0].appendChild(script);

        var script = document.createElement('script');
        script.type = "text/javascript";
        script.src = "/Modules/NKD/Scripts/jquery.validate.unobtrusive.min.js";
        document.getElementsByTagName('head')[0].appendChild(script);

        var script = document.createElement('script');
        script.type = "text/javascript";
        script.src = "/Modules/NKD/Scripts/MicrosoftAjax.js";
        document.getElementsByTagName('head')[0].appendChild(script);

        var script = document.createElement('script');
        script.type = "text/javascript";
        script.src = "/Modules/NKD/Scripts/MicrosoftMvcValidation.js";
        document.getElementsByTagName('head')[0].appendChild(script);

        (function ($) {
            $.fn.cascade = function (options) {
                var defaults = {};
                var opts = $.extend(defaults, options);
                return this.each(function () {
                    $(this).change(function () {
                        var selectedValue = $(this).val();
                        var params = {};
                        params[opts.paramName] = selectedValue;
                        params[opts.customParamName] = $(opts.customValue).val();
                        $.getJSON(opts.url, params, function (items) {
                            opts.childSelect.empty();
                            $.each(items, function (index, item) {
                                opts.childSelect.append(
                                    $('<option/>')
                                        .attr('value', item.Value)
                                        .text(item.Text)
                                );
                            });
                        });
                    });
                });
            };
        })(jQuery);
    });
})();
