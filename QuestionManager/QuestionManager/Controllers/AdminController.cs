using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QuestionManager.Models;
using System.Configuration;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace QuestionManager.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AdminController()
        {
        }

        public AdminController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

        /// <summary>
        /// 管理者用ホーム
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            var keys = Request.QueryString.AllKeys;
            int categoryID = 0;
            foreach (var key in keys)
            {
                categoryID = int.Parse(Request.QueryString.Get(key));
            }
            AdminModels.DisplayAdminIndex model = null;

            AdminIndex index = new AdminIndex();
            model = index.GetAllIndexData(categoryID);

            ViewData["cID"] = categoryID;
            return View(model);
        }

        /// <summary>
        /// パスワードリセット
        /// </summary>
        /// <returns></returns>
        public ActionResult ResetPassword()
        {
            return View();
        }

        /// <summary>
        /// パスワードをリセットする
        /// </summary>
        /// <param name="submit">コマンド</param>
        /// <param name="mail">リセットするアカウント</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ResetPassword(string submit, string mail)
        {
            string status = "パスワードをリセットできませんでした。";
            if (submit == "リセット")
            {
                var r = new ResetPassword();
                status = r.Reset(mail);
            }

            ViewData["status"] = status;
            return View();
        }

        /// <summary>
        /// 管理用画面
        /// </summary>
        /// <returns></returns>
        public ActionResult AdminMenu()
        {
            using (var qe = new QuestionEntities())
            {
                // 削除されていない、一番若いカテゴリIDを取得
                var cat = qe.Categories.Where(x => x.IsDelete == false).OrderBy(x => x.CategoryID).FirstOrDefault();

                if (cat != null)
                {
                    ViewData["categoryID"] = cat.CategoryID;
                }
                else
                {
                    ViewData["categoryID"] = 0;
                }
            }

            return View();
        }

        /// <summary>
        /// ユーザー編集
        /// </summary>
        /// <returns></returns>
        public ActionResult EditUser()
        {
            return View();
        }

        /// <summary>
        /// ユーザー情報をjson形式で取得して、jqgridに表示する
        /// </summary>
        /// <returns></returns>
        public JsonResult GetUser()
        {
            // ユーザー一覧情報を取得
            EditUser e = new EditUser();
            var jsondata = e.GetUser();

            // 取得出来なかった場合エラー表示
            if (jsondata == null)
            {
                // エラー表示
                Response.StatusCode = 404;
            }

            return Json(jsondata, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// jqgrid内ユーザー情報の編集
        /// </summary>
        /// <param name="data">ユーザー入力情報</param>
        /// <returns></returns>
        public JsonResult SaveUser(FormCollection data)
        {
            // エラーメッセージ
            string status = null;

            // 追加・編集・削除処理
            EditUser e = new EditUser();
            switch (data["oper"])
            {
                case "edit":
                    status = e.EditUserData(data);
                    break;
                case "add":
                    status = e.InsertUserData(data);
                    break;
                case "del":
                    status = e.DeleteUserData(data);
                    break;
                default:
                    break;
            }

            // 取得出来なかった場合エラー表示
            if (status != null)
            {
                Response.StatusCode = 404;
            }
            else
            {
                // statusがnullのときは空文字を返す（パーサーエラー回避）
                status = "";
            }

            return Json(status);
        }

        /// <summary>
        /// カテゴリ編集
        /// </summary>
        /// <returns></returns>
        public ActionResult EditCategories()
        {
            // カテゴリ一覧取得
            var cat = new EditCategories();
            var model = cat.GetCategories();

            return View(model);
        }

        /// <summary>
        /// カテゴリ編集
        /// </summary>
        /// <param name="submit">実行コマンド（追加、削除、変更）</param>
        /// <param name="button">実行コマンド</param>
        /// <param name="newCategory">追加するカテゴリ名</param>
        /// <param name="changeCategory">変更するカテゴリ名</param>
        /// <param name="categoryID">カテゴリID</param>
        /// <param name="id">???(教科名？)</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditCategories(string submit, string button, string newCategory, string changeCategory, int? categoryID, int? id)
        {
            // カテゴリ名編集
            string message = null;
            var cat = new EditCategories();
            try
            {
            }
            catch
            {
                return View("Error");
            }

            if (submit == "追加" && newCategory != String.Empty)
            {
                // 追加
                message = cat.AddCategory(newCategory);
            }
            else if (submit == "削除" && categoryID != null)
            {
                // 削除
                cat.DeleteCategory(categoryID);
            }
            else if (button == "変更" && string.IsNullOrEmpty(changeCategory) == false)
            {
                // 課題名を変更
                cat.ChangeCategory(categoryID, changeCategory);

                return Json(id);
            }


            // エラー通知
            if (message != null)
            {
                ViewData["message"] = message;
            }

            // 削除されていない課題名だけ表示
            var model = cat.GetCategories();

            return View(model);
        }

        /// <summary>
        /// 削除されていない課題名だけ表示
        /// </summary>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        public ActionResult EditSubject(int categoryID)
        {
            AdminModels.SubjectsData data = new AdminModels.SubjectsData();

            if (categoryID == 0)
            {
                return View(data);
            }

            // 課題名取得
            var sub = new EditSubject();
            var model = sub.GetSubject(categoryID);

            data.Subjects = model;

            // カテゴリ情報取得
            List<Categories> categories = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                categories = db.Categories.Where(c => c.IsDelete == false).ToList();
            }

            data.Categories = categories;

            // 確認テストの時間制限設定
            int finalID = 0;
            int finalTimeLimit = 0;

            using (QuestionEntities en = new QuestionEntities())
            {
                // 確認テストIDと制限時間の取得
                finalID = en.subjects.Where(x => x.CategoryID == categoryID && x.SubjectName == AdminModels.FINAL_TEST_NAME).SingleOrDefault().SubjectID;
                finalTimeLimit = (int)en.subjects.Where(x => x.CategoryID == categoryID && x.SubjectName == AdminModels.FINAL_TEST_NAME).SingleOrDefault().TimeLimit;
            }

            ViewData["FinalID"] = finalID;
            ViewData["FinalTimeLimit"] = finalTimeLimit;

            // 課題の表示問題数
            // ViewData["SubjectDisplay"] = ConfigurationManager.AppSettings["SubjectDisplay"];

            // 課題の1日受験回数制限を取得して表示用文字列作成
            string displayLimit = "なし";
            if (ConfigurationManager.AppSettings["LimitExam"] == "true")
            {
                displayLimit = "あり";
            }

            // 課題の1日受験回数制限
            ViewData["LimitExam"] = displayLimit;
            // 課題の合格点
            ViewData["PassingMark"] = ConfigurationManager.AppSettings["PassingMark"];
            // 確認テストの合格点
            ViewData["FinalPassingMark"] = ConfigurationManager.AppSettings["FinalPassingMark"];

            return View(data);
        }

        /// <summary>
        /// 課題名
        /// </summary>
        /// <param name="submit">実行コマンド（追加、並べ替え）</param>
        /// <param name="button">実行コマンド（変更、削除）</param>
        /// <param name="submit_pm">課題の合格点の更新</param>
        /// <param name="submit_fpm">確認テストの合格点の更新</param>
        /// <param name="newSubject">追加する課題名</param>
        /// <param name="changeSubject">変更する課題名</param>
        /// <param name="subjectID">課題ID</param>
        /// <param name="id">aspxページでの課題名の表示位置</param>
        /// <param name="PassingMark">課題の合格点</param>
        /// <param name="FinalPassingMark">確認テストの合格点</param>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditSubject(string submit, string button, string submit_le, string submit_pm, string submit_fpm, string submit_time, string submit_finalTime, string newSubject, string changeSubject, int? subjectID, int? id, string PassingMark, string FinalPassingMark, int categoryID, string[] timeLimit, int?[] allSubjectID)
        {
            AdminModels.SubjectsData data = new AdminModels.SubjectsData();

            if (categoryID == 0)
            {
                return View(data);
            }

            // 課題名編集
            var sub = new EditSubject();
            string message = null;

            if (submit == "追加" && newSubject != String.Empty)
            {
                // 追加
                message = sub.AddSubject(newSubject, categoryID);
            }
            else if (submit == "削除" && subjectID != null)
            {
                // 削除
                sub.DeleteSubject(subjectID);
            }
            else if (submit == "▲")
            {
                // 順位を上げる
                sub.ChangeSOrder(subjectID, 1, categoryID);
            }
            else if (submit == "▼")
            {
                // 順位を下げる
                sub.ChangeSOrder(subjectID, -1, categoryID);
            }
            else if (button == "変更" && string.IsNullOrEmpty(changeSubject) == false)
            {
                // 課題名を変更する
                message = sub.ChangeSubject(subjectID, changeSubject, categoryID);
                if (message != null)
                {
                    return Json(null);

                }
                return Json(id);
            }
            else if (submit_le == "変更")
            {
                // 課題の1日受験回数制限の変更
                message = sub.LimitExam();
            }
            else if (submit_pm == "更新")
            {
                // 課題の合格点の更新
                message = sub.PassingMark(PassingMark);
            }
            else if (submit_fpm == "更新")
            {
                // 確認テストの合格点の更新
                message = sub.FinalPassingMark(FinalPassingMark);
            }
            else if (submit_time == "変更")
            {
                // 課題の制限時間の更新
                List<string[]> checkIndex = new List<string[]>();
                for (int i = 0; i < allSubjectID.Length; i++)
                {
                    if (allSubjectID[i].HasValue)
                    {
                        if ((int)allSubjectID[i] == subjectID)
                        {
                            int numberTimeLimit = 0;
                            if (int.TryParse(timeLimit[i], out numberTimeLimit) && 0 < numberTimeLimit)
                            {
                                using (QuestionEntities en = new QuestionEntities())
                                {
                                    var result = en.subjects.Where(x => x.SubjectID == subjectID).SingleOrDefault();
                                    result.TimeLimit = int.Parse(timeLimit[i]);
                                    en.SaveChanges();
                                }
                            }
                            else
                            {
                                message = "制限時間には正しい数値を入力してください。";
                            }
                        }
                    }
                }
            }
            else if (submit_finalTime == "変更")
            {
                if (int.TryParse(timeLimit[timeLimit.Count() - 1], out int n) && 0 < n)
                {
                    using (QuestionEntities en = new QuestionEntities())
                    {
                        var final = en.subjects.Where(x => x.CategoryID == categoryID).Where(x => x.SubjectName == AdminModels.FINAL_TEST_NAME).SingleOrDefault();
                        final.TimeLimit = n;
                        en.SaveChanges();
                    }
                }
                else
                {
                    message = "制限時間には正しい数値を入力してください。";
                }
            }

            // エラー通知
            if (message != null)
            {
                ViewData["message"] = message;
            }

            // 課題名取得
            var model = sub.GetSubject(categoryID);

            data.Subjects = model;

            // カテゴリ情報取得
            List<Categories> categories = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                categories = db.Categories.Where(c => c.IsDelete == false).ToList();
            }

            data.Categories = categories;

            // 確認テストの時間制限設定
            int finalID = 0;
            int finalTimeLimit = 0;

            using (QuestionEntities en = new QuestionEntities())
            {
                // 確認テストIDと制限時間の取得
                finalID = en.subjects.Where(x => x.CategoryID == categoryID && x.SubjectName == AdminModels.FINAL_TEST_NAME).SingleOrDefault().SubjectID;
                finalTimeLimit = (int)en.subjects.Where(x => x.CategoryID == categoryID && x.SubjectName == AdminModels.FINAL_TEST_NAME).SingleOrDefault().TimeLimit;
            }

            ViewData["FinalID"] = finalID;
            ViewData["FinalTimeLimit"] = finalTimeLimit;

            // 課題の表示問題数
            // ViewData["SubjectDisplay"] = ConfigurationManager.AppSettings["SubjectDisplay"];

            System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

            // 課題の1日受験回数制限を取得して表示用文字列作成
            // 変更後のtrue/falseを読み取ってくれないので、変更したときに限っては条件の判定を逆にした
            string displayLimit = string.Empty;
            if (message == "受験回数制限の有無を変更しました。")
            {
                if (ConfigurationManager.AppSettings["LimitExam"] != "true")
                {
                    displayLimit = "あり";
                }
                else
                {
                    displayLimit = "なし";
                }
            }
            else
            {
                if (ConfigurationManager.AppSettings["LimitExam"] == "true")
                {
                    displayLimit = "あり";
                }
                else
                {
                    displayLimit = "なし";
                }
            }

            // 課題の1日受験回数制限
            ViewData["LimitExam"] = displayLimit;
            // 課題の合格点
            ViewData["PassingMark"] = ConfigurationManager.AppSettings["PassingMark"];
            // 確認テストの合格点
            ViewData["FinalPassingMark"] = ConfigurationManager.AppSettings["FinalPassingMark"];

            return View(data);
        }

        /// <summary>
        /// CSVファイルで問題追加
        /// </summary>
        /// <returns></returns>
        public ActionResult InsertQuestion()
        {
            return View();
        }

        /// <summary>
        /// アップロードされたcsvファイルを元に問題をDBに追加する
        /// </summary>
        /// <param name="uploadFile">アップロードされたファイル</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult InsertQuestion(HttpPostedFileWrapper uploadFile)
        {
            // ファイルを元に問題をDBに追加
            InsertQuestionToDB insertQuestion = new InsertQuestionToDB();
            string message = insertQuestion.Insert(uploadFile);

            // 結果を表示
            ViewData["message"] = message;

            return View();
        }

        /// <summary>
        /// 課題受験回数を表示
        /// </summary>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        public ActionResult Level(int categoryID)
        {
            int firstYear = int.Parse(ConfigurationManager.AppSettings["FirstYear"]);
            int finalYear = int.Parse(ConfigurationManager.AppSettings["FinalYear"]);

            // データ取得
            var level = new Level();
            var model = level.MakeList(categoryID, firstYear, finalYear);

            // カテゴリ情報
            List<Categories> categories = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                categories = db.Categories.Where(c => c.IsDelete == false).ToList();
            }
            ViewData["categories"] = categories;

            // データ表示範囲
            ViewData["FirstYear"] = DateTime.Today.Year - 1;
            ViewData["FinalYear"] = DateTime.Today.Year;

            return View(model);
        }

        /// <summary>
        /// 取得する範囲を変えて課題受験回数を表示
        /// </summary>
        /// <param name="first">この年から</param>
        /// <param name="final">この年までに作成されたアカウントのデータを使用</param>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Level(string first, string final, int categoryID)
        {
            // データ範囲指定
            int firstYear = 0;
            int finalYear = 0;
            if (!int.TryParse(first, out firstYear) || !int.TryParse(final, out finalYear))
            {
                return View("Error");
            }

            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings["FirstYear"].Value = first;
            config.AppSettings.Settings["FinalYear"].Value = final;

            // データ取得
            var level = new Level();
            var model = level.MakeList(categoryID, firstYear, finalYear);
            // データ表示範囲
            ViewData["FirstYear"] = ConfigurationManager.AppSettings["FirstYear"];
            ViewData["FinalYear"] = ConfigurationManager.AppSettings["FinalYear"];

            // カテゴリ情報
            List<Categories> categories = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                categories = db.Categories.Where(c => c.IsDelete == false).ToList();
            }
            ViewData["categories"] = categories;

            return View(model);
        }

        /// <summary>
        /// 課題到達度を表示
        /// </summary>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        public ActionResult Progress(int categoryID)
        {
            int firstYear = int.Parse(ConfigurationManager.AppSettings["FirstYear"]);
            int finalYear = int.Parse(ConfigurationManager.AppSettings["FinalYear"]);

            // データ取得
            var progress = new Progress();
            var model = progress.MakeList(categoryID, firstYear, finalYear);

            // カテゴリ情報
            List<Categories> categories = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                categories = db.Categories.Where(c => c.IsDelete == false).ToList();
            }
            ViewData["categories"] = categories;

            // データ表示範囲
            ViewData["FirstYear"] = DateTime.Today.Year - 1;
            ViewData["FinalYear"] = DateTime.Today.Year;
            return View(model);
        }

        /// <summary>
        /// 取得する範囲を変えて課題到達度を表示
        /// </summary>
        /// <param name="first">この年から</param>
        /// <param name="final">この年までに作成されたアカウントのデータを使用</param>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Progress(string first, string final, int categoryID)
        {
            // データ範囲指定
            int firstYear = 0;
            int finalYear = 0;
            if (!int.TryParse(first, out firstYear) || !int.TryParse(final, out finalYear))
            {
                return View("Error");
            }

            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings["FirstYear"].Value = first;
            config.AppSettings.Settings["FinalYear"].Value = final;

            // データ取得
            var progress = new Progress();
            var model = progress.MakeList(categoryID, firstYear, finalYear);

            // カテゴリ情報
            List<Categories> categories = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                categories = db.Categories.Where(c => c.IsDelete == false).ToList();
            }
            ViewData["categories"] = categories;

            // データ表示範囲
            ViewData["FirstYear"] = ConfigurationManager.AppSettings["FirstYear"];
            ViewData["FinalYear"] = ConfigurationManager.AppSettings["FinalYear"];
            return View(model);
        }

        /// <summary>
        /// 受験者名
        /// </summary>
        /// <returns></returns>
        public ActionResult DisplayUsers()
        {
            try
            {
                // カテゴリ情報
                List<Categories> categories = null;
                using (QuestionEntities db = new QuestionEntities())
                {
                    categories = db.Categories.Where(c => c.IsDelete == false).ToList();
                }

                ViewData["categories"] = categories;
                ViewData["categoryID"] = 1;


                // 受験者一覧を表示
                var adminResults = new AdminResults();
                var model = adminResults.GetDisplayUsers();

                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }

        /// <summary>
        /// 受験者の受験履歴
        /// </summary>
        /// <param name="userID">ユーザーID</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        public ActionResult UserSummary(string userID, string userName, int? categoryID)
        {
            List<AdminModels.ResultAllHistory> list = new List<AdminModels.ResultAllHistory>();

            try
            {
                // 各ユーザーの受験履歴を取得
                var adminResults = new AdminResults();
                list = adminResults.GetAdminSummary(userID, (int)categoryID);

                // 教科ID毎にソート
                list = list.OrderBy(x => x.SubjectID).ToList();

                // カテゴリ情報
                var adminCategory = new List<AdminModels.CategoryList>();
                adminCategory = adminResults.GetAdminCategory();

                // カテゴリ名
                string categryName = string.Empty;
                using (QuestionEntities en = new QuestionEntities())
                {
                    categryName = en.Categories.Where(x => x.CategoryID == categoryID).Single().CategoryName;
                }

                var model = new AdminModels.DisplayAdminSummary();
                model.AdminSummaryList = list;
                model.UserName = userName;
                model.UserID = userID;
                model.CategoryID = categoryID;
                model.CategoryName = categryName;
                model.CategoriesList = adminCategory;

                // タイトルを取得する
                ViewData["Title"] = categryName;

                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }

        /// <summary>
        /// 解答履歴
        /// </summary>
        /// <param name="userID">ユーザーID</param>
        /// <param name="subjectID">教科ID</param>
        /// <param name="challengeTimes">受験回数</param>
        /// <param name="subjectName">教科名</param>
        /// <param name="userName">ユーザー名</param>
        /// <param name="categoryID">カテゴリID</param>
        /// <returns></returns>
        public ActionResult UserDetails(string userID, int? subjectID, int? challengeTimes, string subjectName, string userName, int? categoryID)
        {
            List<AdminModels.DisplayAllHistory> list = null;

            try
            {
                // 解答履歴を取得
                var adminResults = new AdminResults();
                list = adminResults.GetAdminDetail(userID, subjectID, challengeTimes, categoryID);

                // カテゴリ名
                string categryName = string.Empty;
                using (QuestionEntities en = new QuestionEntities())
                {
                    categryName = en.Categories.Where(x => x.CategoryID == categoryID).Single().CategoryName;
                }

                var model = new AdminModels.DisplayAdminDetails();
                model.AdminDetailsList = list;
                model.UserName = userName;
                model.UserID = userID;
                model.SubjectName = subjectName;
                model.SubjectID = subjectID;
                model.CategoryName = categryName;
                model.CategoryID = categoryID;
                model.ChallengeTime = challengeTimes;

                // タイトルを取得する
                ViewData["Title"] = categryName;

                return View(model);
            }
            catch
            {
                return View("Error");
            }
        }
    }
}