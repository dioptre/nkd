(function () {

    ////////////////////////////////////////////////////////////////////////
    // NOTE: IF YOU EDIT THIS FILE
    // You must also update editor_plugin.js with a minified version.
    ////////////////////////////////////////////////////////////////////////
    tinymce.create('tinymce.plugins.EXPEDIT.LocationPicker', {
        /**
        * Initializes the plugin, this will be executed after the plugin has been created.
        * This call is done before the editor instance has finished it's initialization so use the onInit event
        * of the editor instance to intercept that event.
        *
        * @param {tinymce.Editor} ed Editor instance that the plugin is initialized in.
        * @param {string} url Absolute URL to where the plugin is located.
        */    
        init: function (ed, url) {
            // Register the command so that it can be invoked by using tinyMCE.activeEditor.execCommand('mceMediaPicker');
            ed.addCommand('mceLocationPicker', function () {
                ed.focus();            
                var url = "/Share/PickLocation/";
                $.colorbox({
                    href: url,
                    iframe: true,
                    reposition: true,
                    width: "90%",
                    height: "90%",
                    onLoad: function() {
                        // hide the scrollbars from the main window
                        $('html, body').css('overflow', 'hidden');
                        //$('#cboxClose').remove();
                    },
                    onClosed: function() {
                        $('html, body').css('overflow', '');

                        var selectedData = $.colorbox.selectedData;

                        if (selectedData == null) // Dialog cancelled, do nothing
                            return;

                        var newContent = '';
                        for (var i = 0; i < selectedData.length; i++) {                            
                            var link = "/share/Location/" + selectedData[i].refid;
                            newContent += "<ul style=\'list-style-type:none !important;\'><li><p class=\"title\"><a href=\'" + link + "\'>" + selectedData[i].name + "</a></p></li></ul>&nbsp;";

   

                            //$.ajax({
                            //    async: false,
                            //    type: 'GET',
                            //    url: renderMedia,
                            //    success: function(data) {
                            //        newContent += data;
                            //    }
                            //});
                        }
                        $.colorbox.selectedData = null;

                        // reassign the src to force a refresh
                        tinyMCE.execCommand('mceReplaceContent', false, newContent);
                    }
                });
            });

            // Register media button
            ed.addButton('locationpicker', {
                title: jQuery("#" + ed.id).data("locationpicker-title"), //ed.getParam("mediapicker_title"),
                cmd: 'mceLocationPicker',
                image: url + '/img/globe_add.png'
            });
        },

        /**
        * Creates control instances based in the incomming name. This method is normally not
        * needed since the addButton method of the tinymce.Editor class is a more easy way of adding buttons
        * but you sometimes need to create more complex controls like listboxes, split buttons etc then this
        * method can be used to create those.
        *
        * @param {String} n Name of the control to create.
        * @param {tinymce.ControlManager} cm Control manager to use inorder to create new control.
        * @return {tinymce.ui.Control} New control instance or null if no control was created.
        */
        createControl: function (n, cm) {
            return null;
        },

        /**
        * Returns information about the plugin as a name/value array.
        * The current keys are longname, author, authorurl, infourl and version.
        *
        * @return {Object} Name/value array containing information about the plugin.
        */
        getInfo: function () {
            return {
                longname: 'EXPEDIT LocationPicker Plugin',
                author: 'Andrew Grosser',
                authorurl: 'http://expedit.com.au',
                infourl: 'http://expedit.com.au',
                version: '0.1'
            };
        }
    });

    // Register plugin
    tinymce.PluginManager.add('locationpicker', tinymce.plugins.EXPEDIT.LocationPicker);
})();