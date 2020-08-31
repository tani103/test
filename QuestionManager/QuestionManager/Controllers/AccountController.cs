using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using QuestionManager.Models;

using System.Web.Security;


namespace QuestionManager.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        private UserEntities userDb = new UserEntities();
        private QuestionEntities questionDb = new QuestionEntities();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }                    
        }

        //
        // GET: /Account/LogOn
        [AllowAnonymous]
        public ActionResult LogOn(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/LogOn
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LogOn(LoginViewModel model, string returnUrl, string email, string userName, string password)
        {
            // Null許容型ではないため、空文字にて初期化
            email = string.Empty;
            password = string.Empty;

            email = model.Email;
            password = model.Password;

            if (email == null || password == null)
            {
                // emailかpasswordがどちらか空文字
                return View(model);
            }

            // 入力されたユーザー名（email）とDBのEmailが一致するか検索、不一致ならnullを返す
            var confirmUserName = userDb.AspNetUsers.FirstOrDefault(a => a.Email == email);

            if (confirmUserName != null)
            {
                // 入力されたユーザー名がDBに存在する、かつパスワードが空文字でない
                // model.Emailと入力されたユーザー名が一致しているレコード取得
                var var1 = userDb.AspNetUsers.Where(x => x.Email == email).First();
                // userNameをレコードのUserNameで初期化（PasswordSignInAsyncはUserName、Passwordでログイン認証をしているため、UserNameをEmailで置き換える）
                userName = var1.UserName;

                // これは、アカウント ロックアウトの基準となるログイン失敗回数を数えません。
                // パスワード入力失敗回数に基づいてアカウントがロックアウトされるように設定するには、shouldLockout: true に変更してください。
                var result = await SignInManager.PasswordSignInAsync(userName, password, model.RememberMe, shouldLockout: true);
                switch (result)
                {
                    case SignInStatus.Success:
                        ViewData["Name"] = var1.UserName;
                        return RedirectToLocal(returnUrl);
                    case SignInStatus.LockedOut:
                        ModelState.AddModelError("", "指定されたユーザー名またはパスワードが正しくありません。");
                        return View(model);
                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                    case SignInStatus.Failure:
                    default:
                        // 入力されたユーザー名がDBに存在するが、パスワードが一致しない

                        // アクセス時刻を取得
                        DateTime accessTime = DateTime.Now;

                        DateTime a = var1.LastAccessDateTime;

                        // アクセス時刻からDBのアクセス時刻を減算
                        TimeSpan span = accessTime - var1.LastAccessDateTime;
                        if (span.TotalMinutes > 10)
                        {
                            // 最初のアクセスから10分以上経過したのでDBのアクセス時刻を更新
                            var1.LastAccessDateTime = accessTime;
                            // アクセスカウントを0にリセット
                            var1.AccessFailedCount = 0;
                            userDb.SaveChanges();
                        }
                        break;
                }
            }
            // 入力されたユーザー名がDBに存在しないかつパスワードが空文字でない
            ModelState.AddModelError("", "指定されたユーザー名またはパスワードが正しくありません。");
            return View(model);
        }

        /// <summary>
        /// メールアドレスを変更する
        /// </summary>
        /// <returns></returns>
        public ActionResult ChangeEmail()
        {
            ViewBag.Name = string.Empty;

            return View();
        }

        /// <summary>
        /// メールアドレスを変更する
        /// </summary>
        /// <param name="submit">コマンド</param>
        /// <param name="oldmail">変更するアドレス</param>
        /// <param name="newmail">新しいアドレス</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeEmail(string submit, string oldmail, string newmail)
        {
            string status = "メールアドレスを変更できませんでした。";

            // ログインユーザーのユーザーIDを取得
            string userId = User.Identity.GetUserId();

            // ユーザーIDと一致するEmailを取得
            var logOnUser = (from a in userDb.AspNetUsers
                             where a.Id == userId
                             select a).First();
            string email = logOnUser.Email;

            if (submit == "変更" && oldmail == email)
            {
                // ログインユーザーのメールアドレスとoldmailが一致
                // メールアドレスの重複はない予定なので、これで他者のメールアドレスを勝手に変更されることはないはず

                var registered = userDb.AspNetUsers.FirstOrDefault(a => a.Email == newmail);
                if (registered != null)
                {
                    // newmailが既に登録済
                    status = "変更先のメールアドレスは既に登録されています。";
                    ViewData["status"] = status;
                    return View();
                }
                // Email更新
                logOnUser.Email = newmail;
                userDb.SaveChanges();

                status = "メールアドレスを" + newmail + "に変更しました。";
            }
            else
            {
                // ログインユーザーのメールアドレスとoldmailが不一致
                status = "メールアドレスが違います。";
            }
            ViewData["status"] = status;
            return View();
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // アカウント確認とパスワード リセットを有効にする方法の詳細については、https://go.microsoft.com/fwlink/?LinkID=320771 を参照してください
                    // このリンクを含む電子メールを送信します
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "アカウントの確認", "このリンクをクリックすることによってアカウントを確認してください <a href=\"" + callbackUrl + "\">こちら</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // ここで問題が発生した場合はフォームを再表示します
            return View(model);
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // ユーザーがユーザー名/パスワードまたは外部ログイン経由でログイン済みであることが必要です。
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 次のコードは、2 要素コードに対するブルート フォース攻撃を防ぎます。
            // ユーザーが誤ったコードを入力した回数が指定の回数に達すると、ユーザー アカウントは
            // 指定の時間が経過するまでロックアウトされます。
            // アカウント ロックアウトの設定は IdentityConfig の中で構成できます。
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "無効なコード。");
                    return View(model);
            }
        }

        //
        // GET: /Account/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // 新しいパスワードか確認用パスワードの文字数が6文字未満
                // ここで問題が発生した場合はフォームを再表示します
                return View(model);
            }

            if (model.NewPassword == model.ConfirmPassword)
            {
                // 新しいパスワードと確認用パスワードが一致
                var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

                if (result.Succeeded)
                {
                    // 現在のパスワードがDBのパスワードと一致

                    // ログインしているユーザーIDを確認している？
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    // パスワード変更、成功画面に遷移
                    return RedirectToAction("ChangePasswordSuccess", "Account");
                }
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                // 現在のパスワードが一致かつ新しいパスワードと確認用パスワードが不一致
                ModelState.AddModelError("", "新しいパスワードと確認のパスワードが一致しません。");
                return View(model);
            }

            // 現在のパスワードが不一致かつ新しいパスワードと確認用パスワードが一致
            ModelState.AddModelError("", "現在のパスワードが正しくないか、新しいパスワードが無効です。");
            return View(model);
        }

        // GET: /Account/ChangePasswordSuccess
        [AllowAnonymous]
        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        /*
                 //
        // POST: /Account/ChangePassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.OldPassword, Email = model.NewPassword };
                var result = await UserManager.CreateAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    // パスワード変更処理
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // アカウント確認とパスワード リセットを有効にする方法の詳細については、https://go.microsoft.com/fwlink/?LinkID=320771 を参照してください
                    // このリンクを含む電子メールを送信します
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = coChangede }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "アカウントの確認", "このリンクをクリックすることによってアカウントを確認してください <a href=\"" + callbackUrl + "\">こちら</a>");

                    return RedirectToAction("Index", "Home");
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    // 現在のパスワードが一致かつ新しいパスワードと確認用パスワードが不一致
                    ModelState.AddModelError("", "新しいパスワードと確認のパスワードが一致しません。");
                    return View(model);
                }

                // 現在のパスワードが不一致かつ新しいパスワードと確認用パスワードが一致
                ModelState.AddModelError("", "現在のパスワードが正しくないか、新しいパスワードが無効です。");

                return View(model);
            }

            if (model.NewPassword.Length < 6 || model.ConfirmPassword.Length < 6)
            {
                // 新しいパスワードか確認用パスワードの文字数が6文字未満
                return View(model);
            }

            // ここで問題が発生した場合はフォームを再表示します
            return View(model);
        }
         */

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // ユーザーが存在しないことや未確認であることを公開しません。
                    return View("ForgotPasswordConfirmation");
                }

                // アカウント確認とパスワード リセットを有効にする方法の詳細については、https://go.microsoft.com/fwlink/?LinkID=320771 を参照してください
                // このリンクを含む電子メールを送信します
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "パスワード", "のリセット <a href=\"" + callbackUrl + "\">こちら</a> をクリックして、パスワードをリセットしてください");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // ここで問題が発生した場合はフォームを再表示します
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // ユーザーが存在しないことを公開しません。
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // 外部ログイン プロバイダーへのリダイレクトを要求します
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // トークンを生成して送信します。
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // ユーザーが既にログインを持っている場合、この外部ログイン プロバイダーを使用してユーザーをサインインします
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // ユーザーがアカウントを持っていない場合、ユーザーにアカウントを作成するよう求めます
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // 外部ログイン プロバイダーからユーザーに関する情報を取得します
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region ヘルパー
        // 外部ログインの追加時に XSRF の防止に使用します
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}