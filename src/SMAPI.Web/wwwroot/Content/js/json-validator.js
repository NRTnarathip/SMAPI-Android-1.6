/* globals $ */

var smapi = smapi || {};

/**
 * UI logic for the JSON validator page.
 * @param {string} urlFormat The URL format for a file, with $schemaName and $id placeholders.
 * @param {string} fileId The file ID for the content being viewed, if any.
 */
smapi.jsonValidator = function (urlFormat, fileId) {
    /**
     * Initialize the JSON validator page.
     */
    var init = function () {
        var input = $("#input");

        // change format
        $("#output #format").on("change", function () {
            var schemaName = $(this).val();
            location.href = urlFormat.replace("$schemaName", schemaName).replace("$id", fileId);
        });

        if (input.length) {
            // upload form
            smapi.fileUpload({
                chooseFileLink: $("#choose-file-link"),
                chooseFileInput: $("#inputFile"),
                contentArea: input,
                submitButton: $("#submit")
            });
        }
    };
    init();
};
