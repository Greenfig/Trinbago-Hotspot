using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace Trinbago_MVC5.Controllers
{
    public class MessageInterface
    {
        protected static string Message(string body)
        {
            return "<html><head><meta name='viewport'content='width=device-width,initial-scale=1.0'></head><body><div class='navbar navbar-default'>" +
                "<div class='container'><a href='https://trinbagohotspot.com'class='navbar-brand logo'><span class='logo'><img src='https://trinbagohotspot.com/Images/TH/TBHS_v2.png'alt='TrinbagoHotspot logo'>" +
                "</span></a></div></div><hr class='topbar'><div class='container'><br><div class='row'><div class='col-sm-12'><div class='row'><div class='col-sm-12 col-md-12'>" +
                "<fieldset><div class='row'><div class='col-md-12'style='margin-bottom:20px;'><strong>Reply From :</strong>&nbsp;" +
                body +
                "<div class='footer'><div class='container'><div class='row'><div class='col-sm-4 col-xs-12'><p><strong>&copy;2017-trinbagohotspot.com</strong></p>" +
                "<p>All rights reserved</p></div><div class='col-sm-8 col-xs-12'><p class='footer-links'><a href='https://trinbagohotspot.com'>Home</a>" +
                "<a href='https://trinbagohotspot.com/Home/Guides'>Tips and Guides</a><a href='https://trinbagohotspot.com/Home/Terms'>Terms and Conditions</a>" +
                "<a href='https://trinbagohotspot.com/Home/Privacy'>Privacy Policy</a><a href='https://trinbagohotspot.com/Home/About'>About Us</a>" +
                "<a href='https://trinbagohotspot.com/Home/Contact'>Help/Contact Us</a></p></div></div></div></div>" +
                "<style>body,html{min-height:100%}fieldset,hr,img{border:0}html{font-family:sans-serif;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%;-moz-text-size-adjust:100%;text-size-adjust:100%;font-size:10px;-webkit-tap-highlight-color:transparent;text-rendering:optimizeLegibility!important;-webkit-font-smoothing:antialiased!important;background-color:#F6F6F6}body{margin:0;min-width:330px!important;font:14px/16px arial,helvetica,clean,sans-serif;font-family:'Helvetica Neue',Helvetica,Arial,sans-serif;font-size:14px;line-height:1.42857143;color:#333;background-color:#fff}footer{display:block}a{background-color:transparent;text-decoration:none;color:#072fff;font-weight:600;font-style:italic}a:hover{outline:0;text-decoration:underline}b,strong{font-weight:700}hr{height:0;-webkit-box-sizing:content-box;-moz-box-sizing:content-box;box-sizing:content-box;margin-top:20px;margin-bottom:20px;border-top:1px solid #eee}fieldset{min-width:0;padding:0;margin:0}@media print{*,:after,:before{color:#000!important;text-shadow:none!important;background:0 0!important;-webkit-box-shadow:none!important;box-shadow:none!important}a,a:visited{color:#337ab7;text-decoration:underline}img{page-break-inside:avoid;max-width:100%!important;vertical-align:middle}h2,p{orphans:3;widows:3}h2{page-break-after:avoid}.navbar{display:none}}*,:after,:before{-webkit-box-sizing:border-box;-moz-box-sizing:border-box;box-sizing:border-box}.h2,h2{font-family:inherit;font-weight:500;line-height:1.1;color:inherit}.navbar-brand,h1{line-height:20px}p{margin:0 0 10px}.navbar,.well{margin-bottom:20px}.container{padding-right:15px;padding-left:15px;margin-right:auto;margin-left:auto;max-width:100%!important}@media (min-width:768px){.container{width:750px}}@media (min-width:992px){.container{width:970px}}@media(min-width:1200px){.container{width:1340px}}.row{margin-right:-15px;margin-left:-15px}.col-md-12,.col-sm-12,.col-sm-4,.col-sm-8,.col-xs-12{position:relative;min-height:1px;padding-right:15px;padding-left:15px}.col-xs-12{float:left;width:100%}@media (min-width:768px){.col-sm-12,.col-sm-4,.col-sm-8{float:left}.col-sm-12{width:100%}.col-sm-8{width:66.66666667%}.col-sm-4{width:33.33333333%}}@media (min-width:992px){.col-md-12{float:left;width:100%}}.navbar{position:relative;min-height:50px;border:1px solid transparent}@media (min-width:768px){.navbar{border-radius:4px}}.navbar-brand{float:left;height:50px;padding:15px;font-size:18px}.navbar-brand:focus,.navbar-brand:hover{text-decoration:none}.navbar-brand>img{display:block}.navbar-default{background-color:#f8f8f8}.well{min-height:20px;padding:19px;background-color:#f5f5f5;border:1px solid #e3e3e3;border-radius:4px;-webkit-box-shadow:inset 0 1px 1px rgba(0,0,0,.05);box-shadow:inset 0 1px 1px rgba(0,0,0,.05)}.container:after,.container:before,.navbar:after,.navbar:before,.row:after,.row:before{display:table;content:' '}.container:after,.navbar:after,.row:after{clear:both}:after,:before{-moz-box-sizing:border-box;-webkit-box-sizing:border-box;box-sizing:border-box}.navbar{background-image:-webkit-linear-gradient(top,#fff 0,#fff 100%);filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#ffffffff', endColorstr='#ffffffff', GradientType=0);border-radius:0;background:0 0}.navbar-default{border:none;margin-top:0;padding-top:10px}a.navbar-brand{margin-top:10px}a.logo{width:326px;padding:0;height:120px}.footer,.header,.marketing{padding-left:0;padding-right:0}.footer{border-top:1px solid #E5E5E5;background:#F7F7F7;color:#777;padding-top:19px;font-size:11px;text-align:center;padding-bottom:20px;margin-top:40px}.footer strong{color:#000}.footer p{margin-bottom:0}.footer .footer-links{float:none}.footer .footer-links a{margin-left:20px;font-size:11px}.footer .footer-links a.active{color:#333}@media (min-width:768px){#listings-page .price{text-align:right}.footer{text-align:left}.footer .footer-links{float:right}}.topbar{border:3px solid #D2160A}h1{font-size:24px;margin-top:10px}h2,h2.formhead{font-size:20px}h2{line-height:18px;margin-top:20px;margin-bottom:20px}h2.formhead{position:relative;z-index:1;overflow:hidden;text-align:center;line-height:25px}h2.formhead:after,h2.formhead:before{position:absolute;top:51%;overflow:hidden;width:50%;height:1px;content:'\a0';background-color:#bcbcbc}h2.formhead:before{margin-left:-50%;text-align:right}</style></body></html>";
        }

        protected static string ReplyToAdMessageBody
        {
            get
            {
                return "<span>{0} ({1}) to your&nbsp;&#34;" +
                    "<a href='{3}'target='_blank'>{4}</a>&#34;&nbsp;Ad!</span></div>" +
                    "<div class='col-sm-12'><div class='well'><fieldset><h2 class='formhead'>Message</h2><div class='col-md-12'style='background-color:white'><span>{2}</span></div></fieldset></div></div>" +
                    "<div class='col-sm-12'><b>You can respond to {0} ({1}) by replying directly to this email.</b></div></div></fieldset></div></div><br></div></div></div>";
            }
        }

        protected static string ForgotPasswordMessageBody
        {
            get
            {
                return "<div class='col-sm-12'><div class='well'><fieldset><h2 class='formhead'>Password Reset</h2><div class='col-md-12'style='background-color:white'>" +
                    "<span>Hello({0}),<br/>To reset your account password go to the following link:<section>{1}</section><br/>If clicking the link does not work, copy & paste this url into your browser:<section>{2}</section><br/><p>Thank you,</p><br/><p>TrinbagoHotspot.com </p></span></div></fieldset></div></div>" +
                    "</div></fieldset></div></div><br></div></div></div>";
            }
        }
        public static string ReplyToAdMessage(ClassifiedAdQ msg)
        {
            return Message(string.Format(ReplyToAdMessageBody, msg.Name, msg.From, msg.Message, msg.ItemUrl, msg.AdTitle));
        }

        public static string ForgotPasswordMessage(string username, string confirmationLink, string confirmationUrl)
        {
            return Message(string.Format(ForgotPasswordMessageBody, username, confirmationLink, confirmationUrl));
        }
    }
}