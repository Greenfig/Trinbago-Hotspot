(function (n, t, i) {
    var r, u = n.getElementsByTagName(t)[0]; n.getElementById(i) || (r = n.createElement(t), r.id = i, r.async = 1, r.src = "//connect.facebook.net/en_US/sdk.js#xfbml=1&version=v2.8&appId=307682596258447", u.parentNode.insertBefore(r, u))
})(document, "script", "facebook-jssdk");
(function() {
    var n = document.createElement("script"),
        t; n.type = "text/javascript"; n.async = !0; n.src = "https://apis.google.com/js/platform.js"; t = document.getElementsByTagName("script")[0]; t.parentNode.insertBefore(n, t)
})(document, "script", "google-apis");
(function (n, t, i) {
    var r, u = n.getElementsByTagName(t)[0]; n.getElementById(i) || (r = n.createElement(t),
        r.id = i, r.async = 1, r.src = "//platform.twitter.com/widgets.js", u.parentNode.insertBefore(r, u))
})(document, "script", "twitter-wjs");
function facebookShare(url) {
    FB.ui({
        method: 'share',
        href: $(location).attr('href'),
    }, function (response) { });
};
function twitterShare(url) {
    javascript: window.open('https://twitter.com/share', '', 'menubar=no,toolbar=no,resizable=yes,scrollbars=yes,height=600,width=600');
    return false;
};
function googleShare(url) {
    javascript: window.open('https://plus.google.com/share?url=' + url, '', 'menubar=no,toolbar=no,resizable=yes,scrollbars=yes,height=600,width=600');
    return false;
};