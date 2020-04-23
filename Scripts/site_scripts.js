// SICKY SEARCH
var mobileAdBar = document.getElementById("mobileAdBar");
if (mobileAdBar) {
    var sticky = mobileAdBar.offsetTop;
    window.onscroll = function () { homeTronSearch() };   
    function homeTronSearch() {
        if (window.pageYOffset >= sticky) {
            mobileAdBar.classList.add("mobile-addetails-bar-stick");
            $("#listings-page").addClass("container-pad");
        } else {
            mobileAdBar.classList.remove("mobile-addetails-bar-stick");
            $("#listings-page").removeClass("container-pad");
        }
    }
    $(function () {
        if (window.pageYOffset >= sticky) {
            mobileAdBar.classList.add("mobile-addetails-bar-stick");
            $("#listings-page").addClass("container-pad");
        } else {
            mobileAdBar.classList.remove("mobile-addetails-bar-stick");
            $("#listings-page").removeClass("container-pad");
        }
    });
}
// Get Region with Default
function FetchRegion() {
    var t = "/ClassifiedAd/FetchRegions", n = $("#RegionId");
    $.ajaxSettings.url;
    $("#CountryId").on("change", function () {
        if (n.empty(), $(this).val() == 0) {
            n.append($("<option><\/option>").val(0).text("-No Country Set-"));
            return;
        }
        n.append($("<option><\/option>").val(0).text("-Region-"));
        $.getJSON(t, { ID: $(this).val() }, function (t) {
            $.each(t, function (t, i) {
                n.append($("<option><\/option>").val(i.ID).text(i.Name));
            });
        });
    });
}
function FetchRegionForms() {
    var t = "/ClassifiedAd/FetchRegions", n = $("#RegionId");
    $.ajaxSettings.url;
    $("#CountryId").on("change", function () {
        if (n.empty(), !$(this).val()) {
            n.append($("<option><\/option>").val("").text("-Please select-"));
            return;
        }
        n.append($("<option><\/option>").val("").text("-Please select-"));
        $.getJSON(t, { ID: $(this).val() }, function (t) {
            $.each(t, function (t, i) {
                n.append($("<option><\/option>").val(i.ID).text(i.Name));
            });
        });
    });
}
$(function () {
    $("#adTypeId").on("change", function (n) {
        var i = String(n.target.value), r = $("#priceOptionId"), u, t = document.getElementById("Price");
        if (i == "TRADE") {
            r.val("Please Contact").change();
            r.attr("disabled", true).change();
            t = document.getElementById("Price");
            t.value = null;
            t.value = "Please Contact";
            t.readOnly = !0;
        }
        else if (i == "SELL" || i == "WANT") {
            if (r.is(":disabled")) {
                r.val("None").change();
                r.attr("disabled", false);
                t.value = null;
                t.readOnly = !1;
            }
        }
    });
});
$(function () {
    $("#priceOptionId").on("change", function (n) {
        var i = $(n.target).val(),
            t; i == "None" ? (t = document.getElementById("Price"),
                t.value == "Please Contact" && (t.value = null, t.readOnly = !1)) : i == "Please Contact" ? (t = document.getElementById("Price"),
                    t.value = null, t.value = "Please Contact", t.readOnly = !0) : i == "Non-Negotiable" ? (t = document.getElementById("Price"),
                        t.value == "Please Contact" && (t.value = null, t.readOnly = !1)) : i == "Negotiable" && (t = document.getElementById("Price"),
                            t.value == "Please Contact" && (t.value = null, t.readOnly = !1));
    });
});
$(function () {
    $('[id^="thumbbut"]').on("click", function (n) {
        var r = String(n.target.id),
            t = r.replace("thumbbut", ""),
            i = document.getElementById("output" + t),
            u; i.getAttribute("src") != "/images/TH/noimage.png" && (i.setAttribute("src", "/images/TH/noimage.png"),
                u = document.getElementById("PhotoUpload" + t));
    });
});
$(function () {
    $("#makeId").on("change", function (n) {
        $(n.target).valid();
    });
});
$(function () {
    $('#AdContactEmail').bind('input', function () {
        $(this).val(function (_, v) {
            return v.trim();
        });
    });
});
$(function () {
    $('#AdContactPhone').bind('input', function () {
        $(this).val(function (_, v) {
            return v.trim();
        });
    });
});
$(function () {
    $('#AdContactPhone2').bind('input', function () {
        $(this).val(function (_, v) {
            return v.trim();
        });
    });
});
$(function () {
    $('#AdContactPhone3').bind('input', function () {
        $(this).val(function (_, v) {
            return v.trim();
        });
    });
});
// Index on page load
function indexPage() {
    $(document).ready(function () {
        //always activate first tab
        $('#myTab a:eq(2)').tab('show');
    });
    FetchRegion();
}
// AdDetails on page load
function listDetailPage(hasPhoto, isJob, viewBagMessage) {
    $(document).ready(function () {
        var venoOptions = {
            numeratio: true,
            infinigall: true,
            spinner: 'wave',
            border: '20px',
            titleattr: 'data-title'
        };
        if (hasPhoto) {
            $('.venobox').venobox(venoOptions);
        }

        $(".owl-carousel").owlCarousel({
            nav: true,
            dots: false,
            autoWidth: true
        });

        if (viewBagMessage) {
            $('#myModalMessage').modal('show');
        }

        if (isJob) {
            $("#applyto").click(function (event) {
                var divtag = document.getElementById("applyarea");
                if (divtag.hidden) {
                    divtag.removeAttribute("hidden");
                    document.getElementById("applyto").innerHTML = "Close<br/>Application Form";
                    document.getElementById("Message").focus();
                }
                else {
                    divtag.setAttribute("hidden", "hidden");
                    document.getElementById("applyto").innerHTML = "Open<br/>Application Form";
                }
            });
        }
    });
    FetchRegion();
}
// AdList Page
function adListPage(hasFeatured) {
    var defWidth = $(window).innerWidth();

    // page load
    if ($(this).innerWidth() <= 991) {
        $('.sidebar').addClass('collapse');
        $('.sidebar').removeClass('sidebar-hide');
    }
    else {
        $('.sidebar').removeClass('collapse');
    }
    // page resize
    $(window).bind('resize', function () {
        if ($(this).innerWidth() != defWidth) {
            defWidth = $(window).innerWidth();
            if ($(this).innerWidth() <= 991) {
                $('.sidebar').addClass('collapse');
                $('.sidebar').removeClass('sidebar-hide');
            }
            else {
                $('.sidebar').removeClass('collapse');
            }
        }
    });
    $(document).ready(function () {
        $("#filterForm").submit(function () {
            $("input").each(function (index, obj) {
                if ($(obj).val() == "") {
                    $(obj).remove();
                }
            });
            $("select").each(function (index, obj) {
                if ($(obj).val() == "" || $(obj).val() == "All Ads" || $(obj).val() == "ALL") {
                    $(obj).remove();
                }
            });
        });
        //search model
        $.validator.setDefaults({ ignore: ":hidden:not(#makeIdList)" });

        // div click
        $('[id=viewalllink]').on("click", function (evt) {
            window.location = $(this).attr("data-url-href");
        });
        $('[id=favourite]').on("click", function (evt) {
            var favbtn = $(this);
            evt.stopPropagation();
            $.ajax({
                type: "POST",
                url: "/User/Favourite",
                data: { adId: favbtn.attr("data-fav-id") },
                success: function (json) {
                    if (json.success) {
                        if (json.isFavourited) {
                            favbtn.addClass("active");
                        }
                        else {
                            favbtn.removeClass("active");
                        }
                    }
                }
            });
        });
    });
    FetchRegion();
}
// MyAdList
//triggered when modal is about the be shown
$(function () {
    $('#myModalOpenRequest').on('show.bs.modal', function (e) {
        //get data-id attribute of the clicked element
        var adId = $(e.relatedTarget).data('ad-id');
        //populate the textbox
        $(e.currentTarget).find('#adId').val(adId);
    });

    //triggered when modal is about to be shown
    $('#myModalClose').on('show.bs.modal', function (e) {
        //get data-id attribute of the clicked element
        var adId = $(e.relatedTarget).data('ad-id');
        var adTitle = $(e.relatedTarget).data('ad-title');
        //populate the textbox
        $(e.currentTarget).find('#adtitle').text(adTitle);
        $(e.currentTarget).find('#adId').val(adId);
    });

    $('#myModalAdPromote').on('show.bs.modal', function (e) {
        //get data-id attribute of the clicked element
        var adId = $(e.relatedTarget).data('ad-id');
        var adTitle = $(e.relatedTarget).data('ad-title');
        //populate the textbox
        $(e.currentTarget).find('#adtitle').text(adTitle);
        $(e.currentTarget).find('#adId').val(adId);
    });

    $('#myModalClose').on('click', '#okIdButtonFailure', function () {
        location.reload();
    });

    $('#myModalClose').on('click', '#okIdButtonSucceed', function () {
        location.reload();
    });

    $("#searchType").change(function () {
        $.ajax({
            url: 'MyAdListPagination',
            contentType: 'application/html; charset=utf-8',
            type: 'GET',
            dataType: 'html',
            data: { searchType: this.value },
            success: function (result) {
                $("#ajaxAdList").html(result);
            }
        });
    });
});
// CategorySelectAd on load
function categorySelectPage() {
    $(document).ready(function () {
        var defWidth = $(window).innerWidth();

        if ($(this).width() > 1000) {
            $('.panel-collapse').removeClass('collapse');
        }
        else {
            $('.panel-collapse').removeClass('show').addClass('collapse').css('height', 'auto');
        }

        $(window).bind('resize', function () {
            if ($(this).innerWidth() != defWidth) {
                defWidth = $(window).innerWidth();
                if ($(this).width() > 1000) {
                    $('.panel-collapse').removeClass('collapse');
                }
                else {
                    $('.panel-collapse').removeClass('show').addClass('collapse').css('height', 'auto');
                }
            }
        });
    });
}
// Admin LisDetail image loading
function AdminListDetailPage() {
    $(document).ready(function () {
        var init = document.getElementById('DetailsPhoto');
        var img = document.getElementById('doutput');
        var src = $(init).attr('src');
        if (src != null)
            img.setAttribute("src", src);

        //set default
        var def = document.getElementById('DIContainer');
        def.setAttribute("href", src);
    });
}
// My Favourite page
function removeFavourite() {
    $(document).on("click", '[id=removefavourite]', {}, function (evt) {
        var favbtn = $(this);
        $.ajax({
            type: "POST",
            url: "/User/RemoveFavourite",
            data: { adId: favbtn.attr("data-fav-id"), pageNumber: favbtn.attr("data-pagenumber") },
            dataType: "html",
            success: function (data) {
                $("#ajaxAdList").html(data);
            }
        });
    });
}
// AdDetails page
function adDetailsFavourite(adId) {
    var favbtn = $("#ad-favourited");
    $.ajax({
        type: "POST",
        url: "/User/Favourite",
        data: { adId: adId },
        success: function (json) {
            if (json.success) {
                if (json.isFavourited) {
                    favbtn.addClass("active");
                }
                else {
                    favbtn.removeClass("active");
                }
            }
        }
    });
}