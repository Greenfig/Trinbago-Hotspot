var PhotoUploader = function () {
    'use-strict';
    var _max,
        _currentPhotoCount = 0,
        _currentHtmlTag = 0,
        _rvt = '__RequestVerificationToken',
        _mpc = 'MaxPhotoCount',
        _token,
        _createUrl = '/ClassifiedAd/Manage/UploadPhotoAdd',
        _deleteUrl = '/ClassifiedAd/Manage/UploadPhotoDelete',
        _strid = 'StringId',
        _stringid,
        _photos = 'Photos',
        _cpc = 'CurrentPhotoCount',
        _fileSelect,
        _fileUploadBtn,
        _progressBar,
        _progressBarContainer,
        _stopUpload;

    function init(maxphotos, cpc) {
        _stringid = document.getElementById('StringId').value;
        _fileSelect = $("#file-select");
        _fileUploadBtn = $("#fileupload-btn");
        _progressBar = $("#progressbar");
        _progressBarContainer = $("#progressbar-container");
        _stopUpload = $("#stopupload");
        if (maxphotos != undefined && cpc == undefined) {
            c_init(maxphotos);
        }
        if (maxphotos != undefined && cpc != undefined) {
            e_init(maxphotos, cpc);
        }
        $(".uploaded-photo-view").sortable({
            cursor: "move",
            placeholder: "uploaded-placeholder",
            opacity: 0.6,
            tolerance: "pointer"
        });
    };
    function c_init(maxphotos) {
        _max = maxphotos;
        _token = document.getElementById("create-ad-form").elements['__RequestVerificationToken'].value;
        bindFileSelect();
        bindSubmit("#create-ad-form");

    };
    function e_init(maxphotos, cpc) {
        _max = maxphotos;
        _currentPhotoCount = cpc;
        _currentHtmlTag = cpc;
        _token = document.getElementById("edit-ad-form").elements['__RequestVerificationToken'].value;
        bindFileSelect();
        bindControls();
        bindSubmit("#edit-ad-form");
    };
    function bindFileSelect() {
        _fileSelect.on("change", function (max) {
            var files = _fileSelect[0].files;
            if (files.length == 0 || _max - _currentPhotoCount == 0) return;
            var formData = new FormData();
            formData.append(_rvt, _token);
            formData.append(_strid, _stringid);
            formData.append(_cpc, _currentPhotoCount);
            formData.append(_mpc, _max);
            // Loop through each of the selected files.
            for (var i = 0; i < files.length && _currentPhotoCount <= _max && i < _max - _currentPhotoCount; i++) {
                var file = files[i];
                // Check the file type.
                if (!file.type.match('image.*')) {
                    continue;
                }
                // Add the file to the request.
                formData.append(_photos, file, file.name);
            }
            // Add the ad StringId
            var myxhr = new window.XMLHttpRequest();
            $.ajax({
                xhr: function () {
                    //Upload progress
                    myxhr.upload.addEventListener("progress", function (evt) {
                        if (evt.lengthComputable) {
                            var percentComplete = evt.loaded / evt.total;
                            //Do something with upload progress
                            // update progressbar
                            var per = parseInt(percentComplete * 100);
                            _progressBar.css("width", per + "%");
                            _progressBar.html(per + "%");
                            // when percent is equal to 98% disable stop upload
                            if (per == 95) {
                                _stopUpload.attr("disabled", true);
                            }
                            //console.log(percentComplete);
                        }
                    }, false);
                    //Download progress
                    myxhr.addEventListener("progress", function (evt) {
                        if (evt.lengthComputable) {
                            var percentComplete = evt.loaded / evt.total;
                            //Do something with download progress
                            //console.log(percentComplete);
                        }
                    }, false);
                    myxhr.upload.addEventListener("abort", function (evt) {
                        // hide progress bar
                        _progressBarContainer.removeClass("fade-visible");
                        _progressBarContainer.addClass("fade-hidden");
                        // hide stop upload
                        _stopUpload.removeClass("fade-visible");
                        _stopUpload.addClass("fade-hidden");
                        // Clear input
                        _fileSelect.val("");
                        _fileSelect.attr("disabled", false);
                        _fileUploadBtn.addClass("btn-primary");
                        _fileUploadBtn.removeClass("btn-info");
                        _fileUploadBtn.html(("<span style='padding-right:2px;' class='fa fa-folder-open'></span>Add Photos " + _currentPhotoCount + "/" + _max));
                        // reset progress bar counter
                        _progressBar.css("width", "0%");
                        _progressBar.html("0%");
                    }, false);
                    myxhr.upload.addEventListener("load", function (evt) {
                        // hide progress bar
                        _progressBarContainer.removeClass("fade-visible");
                        _progressBarContainer.addClass("fade-hidden");
                        // hide stop upload
                        _stopUpload.removeClass("fade-visible");
                        _stopUpload.addClass("fade-hidden");
                        // Disable input
                        $("#file-select").attr("disabled", true);
                        // reset progress bar counter
                        _progressBar.css("width", "0%");
                        _progressBar.html("0%");
                    }, false);
                    return myxhr;
                },
                url: _createUrl,
                type: 'POST',
                contentType: false,
                processData: false,
                data: formData,
                beforeSend: function () {
                    // show progress bar
                    _progressBarContainer.removeClass("fade-hidden");
                    _progressBarContainer.addClass("fade-visible");
                    _fileSelect.attr("disabled", true);
                    // show stop upload
                    _stopUpload.removeClass("fade-hidden");
                    _stopUpload.addClass("fade-visible");
                    _stopUpload.one("click", function () {
                        myxhr.abort();
                    });
                    // Change loading bar
                    _fileUploadBtn.addClass("btn-info");
                    _fileUploadBtn.removeClass("btn-primary");
                    _fileUploadBtn.html(("<span style='padding-right:2px;' class='fa fa-spinner fa-spin'></span> Loading..... "));
                },
                success: function (json) {
                    if (json.success) {
                        // append to model
                        var phos = document.getElementById("Photos");
                        var jsonPhotos = JSON.parse(json.photos);
                        var photocount = json.photocount;
                        var photoview = document.querySelector(".uploaded-photo-view");
                        var photohtml = '';
                        for (var i = 0; i < jsonPhotos.length; i++) {
                            var j = jsonPhotos[i];
                            // add to the main create ad form
                            photohtml +=
                                '<li id= "uploaded-' + _currentHtmlTag + '">' +
                                '<div id="thumbnail-decoration-' + _currentHtmlTag + '">' +
                                '<div id="removeimg' + _currentHtmlTag + '" class="removeimg" data-idx="' + _currentHtmlTag + '"></div>' +
                                '<div class="mainribbon"></div>' +
                                '<img id="img-' + _currentHtmlTag + '" style="max-width:100%;height:100px;width:auto;margin-left:auto;margin-right:auto;display:block;" src="' + j["Src"] + '" alt="uploaded image">' +
                                '<div class="col-12" style="border-top:double gainsboro;">' +
                                '<div class="row">' +
                                '<label class="m-0 col-12" style="overflow:hidden;height:35px;border-radius:0;border-bottom:double gainsboro;" value="' + j["Original_FileName"] + '" type="text">' + j["Original_FileName"] + '</label>' +
                                '<div style="height:30px;width:100%;">' +
                                '<div id="makemain' + _currentHtmlTag + '" data-idx="' + _currentHtmlTag + '"></div>' +
                                '<div id="moveup' + _currentHtmlTag + '" data-idx="' + _currentHtmlTag + '"></div>' +
                                '<div id="movedown' + _currentHtmlTag + '" data-idx="' + _currentHtmlTag + '"></div>' +
                                '</div>' +
                                '</div>' +
                                '</div>' +
                                '</div>' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__Index" name="Photos.Index" value="' + _currentHtmlTag + '"/>' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__Original_FileName" name="Photos[' + _currentHtmlTag + '].Original_FileName" value="' + j["Original_FileName"] + '" />' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__AdList_FileName" name="Photos[' + _currentHtmlTag + '].AdList_FileName" value="' + j["AdList_FileName"] + '" />' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__AdDetails_FileName" name="Photos[' + _currentHtmlTag + '].AdDetails_FileName" value="' + j["AdDetails_FileName"] + '" />' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__Raw_FileName" name="Photos[' + _currentHtmlTag + '].Raw_FileName" value="' + j["Raw_FileName"] + '" />' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__ContentType" name="Photos[' + _currentHtmlTag + '].ContentType" value="' + j["ContentType"] + '" />' +
                                '<input hidden id="Photos_' + _currentHtmlTag + '__PhotoLayoutIndex" name="Photos[' + _currentHtmlTag + '].PhotoLayoutIndex" value=""/>' +
                                '</li>';
                            _currentHtmlTag++;
                        }
                        _currentPhotoCount = parseInt(photocount.current);
                        // add to final innerHTML block
                        photoview.innerHTML += photohtml;
                    }
                    else {

                    }
                    // hide progress bar
                    _progressBarContainer.removeClass("fade-visible");
                    _progressBarContainer.addClass("fade-hidden");
                    // hide stop upload
                    _stopUpload.removeClass("fade-visible");
                    _stopUpload.addClass("fade-hidden");
                    // Clear input
                    _fileSelect.val("");
                    _fileSelect.attr("disabled", false);
                    // Change loading bar
                    _fileUploadBtn.addClass("btn-primary");
                    _fileUploadBtn.removeClass("btn-info");
                    _fileUploadBtn.html(("<span style='padding-right:2px;' class='fa fa-folder-open'></span>Add Photos " + _currentPhotoCount + "/" + _max));
                    // reset progress bar counter
                    _progressBar.css("width", "0%");
                    _progressBar.html("0%");
                    bindControls();
                },
                error: function () { }
            });
        });
    };
    function bindControls() {
        // delete
        $("[id^=removeimg]").one("click", function (evt) {
            var index = evt.target.attributes.getNamedItem('data-idx').value;
            // build form data to post
            var formData = new FormData();
            formData.append(_rvt, _token);
            formData.append(_cpc, _currentPhotoCount);
            formData.append("AdList_FileName", document.getElementById("Photos_" + index + "__AdList_FileName").value);
            formData.append("AdDetails_FileName", document.getElementById("Photos_" + index + "__AdDetails_FileName").value);
            formData.append("Raw_FileName", document.getElementById("Photos_" + index + "__Raw_FileName").value);
            formData.append(_strid, _stringid);
            $.ajax({
                url: _deleteUrl,
                type: 'POST',
                contentType: false,
                processData: false,
                dataType: "json",
                data: formData,
                success: function (json) {
                    if (json.success) {
                        // remove from dom
                        var div = document.getElementById("uploaded-" + index);
                        var photocount = json.photocount;
                        div.parentNode.removeChild(div);
                        _currentPhotoCount = parseInt(photocount.current);
                        _fileUploadBtn.html(("<span style='padding-right:2px;' class='fa fa-folder-open'></span>Add Photos " + _currentPhotoCount + "/" + _max));
                        return;
                    }
                    document.getElementById("removeimg" + index).setAttribute("onclick", "removePhoto(" + index + ")");
                    return alert(json.message);
                },
                error: function () {
                    document.getElementById("removeimg" + index).setAttribute("onclick", "removePhoto(" + index + ")");
                    return null;
                }
            });
        });
        // forward
        $("[id^=moveup]").on("click", function (evt) {
            var htmltagcount = evt.target.attributes.getNamedItem('data-idx').value;
            var current = $(".uploaded-photo-view").find("#uploaded-" + htmltagcount)[0];
            var prev = $(current).prev()[0];
            swapPhotos(prev, current);
        });
        // down
        $("[id^=movedown]").on("click", function (evt) {
            var htmltagcount = evt.target.attributes.getNamedItem('data-idx').value;
            var current = $(".uploaded-photo-view").find("#uploaded-" + htmltagcount)[0];
            var next = $(current).next()[0];
            swapPhotos(next, current);
        });
        // main
        $("[id^=makemain]").on("click", function (evt) {
            var htmltagcount = evt.target.attributes.getNamedItem('data-idx').value;
            var first = $(".uploaded-photo-view li:nth-child(1)")[0];
            var current = $(".uploaded-photo-view").find("#uploaded-" + htmltagcount)[0];
            swapPhotos(first, current);
        });
    };
    function swapPhotos(a, b) {
        if (a != null && b != null && a !== b) {
            var next_a = $(a).next()[0];
            var front_b = $(b).prev()[0];
            if (next_a == b && front_b == a) {
                $(b).after(a);
            }
            else if (next_a == null && front_b == null) {
                $(a).after(b);
            }
            else {
                $(next_a).before(b);
                $(front_b).after(a);
            }
        }
    };
    function bindSubmit(idTag) {
        $(idTag).submit(function () {
            var pli = $("[id$=__PhotoLayoutIndex]");
            var counter = 0;
            pli.each(function () {
                $(this).attr("value", counter++);
            });
        });
    }
    return {
        init: init
    };
}();
