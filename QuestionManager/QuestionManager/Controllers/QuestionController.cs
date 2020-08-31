using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QuestionManager.Models;
using System.Data.Entity.Validation;
using System.IO;

namespace QuestionManager.Controllers
{
    // 未ログインだとログオン画面に飛ぶ設定
    [Authorize]
    public class QuestionController : Controller
    {
        private QuestionEntities db = new QuestionEntities();

        // GET: Question
        public ActionResult Index()
        {
            List<index> model = null;
            // Isdeleteがfalseの問題のみ抽出
            model = db.Database.SqlQuery<index>(@"
                                                                                SELECT
                                                                                subjects.SubjectName AS SubjectName,
                                                                                questions.question AS Question,
                                                                                questions.Answer AS Answer,
                                                                                questions.QuestionID AS QuestionID
                                                                           FROM questions INNER JOIN subjects ON questions.SubjectID = subjects.SubjectID
                                                                           WHERE (questions.Isdelete = 'false') AND (subjects.Isdelete = 'false')
                                                                           ORDER BY subjects.SubjectOrder;").ToList();
            // 抽出した問題を表示
            return View(model);
        }

        // GET: Question/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            questions questions = db.questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // GET: Question/Create
        public ActionResult Create()
        {
            // Isdeleteがfalseのカテゴリ抽出
            var Categories = (from a in db.Categories
                           where a.IsDelete== false
                           select a);

            // Isdeleteがfalseと確認テスト以外の教科名抽出
            var subject = (from a in db.subjects
                           where a.Isdelete == false
                           where a.SubjectName != "確認テスト"
                           select a);


            // ドロップダウンリスト用
            ViewBag.CategoryName = new SelectList(Categories, "CategoryID", "CategoryName");
            ViewBag.SubjectID = subject;

            var model = new questions();

            return View();
        }

        // POST: Question/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QuestionID,SubjectID,question,Answer,Isdelete")] questions questions, drop drop, FormCollection collection, HttpPostedFileWrapper fl)
        {
            try
            {
                // FormCollectionの用途は不明
                if (ModelState.IsValid)
                {
                    if (fl != null)
                    {
                        // QuestionIDの最大値 + 1の値を取得
                        int maxId = (int)db.Database.SqlQuery<int>(@"
                                                            SELECT MAX(QuestionID)
                                                            FROM Questions;").Single() + 1;

                        // 相対パスに書き換える
                        string upfile = Server.MapPath("~/Content/Picture/") + "Question" + maxId + Path.GetExtension(fl.FileName);

                        // ファイル保存
                        fl.SaveAs(upfile);
                    }

                    // 問題追加
                    db.questions.Add(questions);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                // カテゴリ名、課題名が未選択のとき用の処理。alertがあるので必要ないと思うけど念のため
                // Isdeleteがfalseの問題のみ抽出
                var subject1 = (from a in db.subjects
                                where a.Isdelete == false
                                select a);

                // ドロップダウンリスト用
                ViewBag.CategoryName = new SelectList(db.Categories, "CategoryID", "CategoryName");
                ViewBag.SubjectID = subject1;

                return View();
            }
            // Isdeleteがfalseの問題のみ抽出
            var subject = (from a in db.subjects
                           where a.Isdelete == false
                           select a);

            // ドロップダウンリスト用
            ViewBag.CategoryName = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.SubjectID = subject;

            return View();
        }

        // GET: Question/Edit/5
        public ActionResult Edit(int? id, edit edit)
        {
            // Isdeleteがfalseの問題のみ抽出
            var subject = (from a in db.subjects
                           where a.Isdelete == false
                           select a);

            // ドロップダウンリスト用
            ViewBag.CategoryName = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.SubjectID = subject;

            QuestionManager.Models.edit model = new QuestionManager.Models.edit();
            // 問題取得
            var list = db.questions.Where(x => x.QuestionID == id).First();

            // 選択した問題のQuestionIDを取得
            string PicturePath = "Question" + list.QuestionID.ToString();

            // Pictureフォルダにある画像ファイルを全て取得
            string[] fileName = System.IO.Directory.GetFiles(Server.MapPath("~/Content/Picture/"));

            model.PicturePath = string.Empty;
            for (int i = 0; i < fileName.Length; i++)
            {
                // 画像ファイル名を取得
                string a = System.IO.Path.GetFileNameWithoutExtension(fileName[i]);
                // 選択した問題のファイル名がPictureフォルダにあるか検索
                if (a == PicturePath)
                {
                    model.PicturePath = a;
                    break;
                }
            }

            model.QuestionID = list.QuestionID;
            model.SubjectID = list.SubjectID;
            model.question = list.question;
            model.Answer = list.Answer;
            return View(model);
        }

        // POST: Question/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionID,SubjectID,question,Answer,Isdelete")] questions questions, drop drop, FormCollection collection, HttpPostedFileWrapper fl, int? id, bool? isDelete)
        {
            try
            {
                // FormCollectionの用途は不明
                string upfile = null;
                string inputPicturePath = string.Empty;
                if (ModelState.IsValid)
                {
                    if (isDelete == null)
                    {
                        if (fl != null)
                        {
                            if (fl.ContentType.StartsWith("image/"))
                            {
                                // 保存先のフォルダに同名の画像があった場合、それを削除する
                                System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".png");
                                System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".jpg");
                                System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".gif");
                                System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".bmp");

                                // アップロード先のパスを生成
                                upfile = Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + Path.GetExtension(fl.FileName);
                                fl.SaveAs(upfile);

                                // DBに画像データを呼び出すための相対パスを登録する準備
                                inputPicturePath = "/Content/Picture/Question" + id.ToString() + Path.GetExtension(fl.FileName);
                            }
                            else
                            {
                                // 画像以外を登録しようとした場合、画像の登録をせず、登録する画像のファイルパスは元のままにする
                                inputPicturePath = db.questions.Where(x => x.QuestionID == id).Single().PicturePath;
                            }
                        }
                        else
                        {
                            {
                                // 何も画像を選択しなかった場合、登録する画像のファイルパスは元のままにする
                                inputPicturePath = db.questions.Where(x => x.QuestionID == id).Single().PicturePath;
                            }
                        }
                    }
                    else
                    {
                        // 保存先のフォルダに同名の画像があった場合、それを削除する
                        System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".png");
                        System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".jpg");
                        System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".gif");
                        System.IO.File.Delete(Server.MapPath("~/Content/Picture/") + "Question" + id.ToString() + ".bmp");

                        inputPicturePath = null;
                    }
                    // 問題更新
                    var data = db.questions.Where(x => x.QuestionID == id).First();
                    data.SubjectID = Int32.Parse(collection["SubjectID"]);
                    data.question = collection["Question"];
                    data.Answer = collection["Answer"];
                    data.PicturePath = string.Empty;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                // カテゴリ名、課題名が未選択のとき用の処理
                QuestionManager.Models.edit model = new QuestionManager.Models.edit();
                model.PicturePath = string.Empty;

                // Isdeleteがfalseの問題のみ抽出
                var subject = (from a in db.subjects
                               where a.Isdelete == false
                               select a);

                // ドロップダウンリスト用
                ViewBag.CategoryName = new SelectList(db.Categories, "CategoryID", "CategoryName");
                ViewBag.SubjectID = subject;

                return View(model);
            }
            // カテゴリ名、課題名が未選択のとき用の処理。未選択時はcatchにいくと思うが念のため
            QuestionManager.Models.edit model1 = new QuestionManager.Models.edit();
            model1.PicturePath = string.Empty;

            // Isdeleteがfalseの問題のみ抽出
            var subject1 = (from a in db.subjects
                            where a.Isdelete == false
                            select a);

            // ドロップダウンリスト用
            ViewBag.CategoryName = new SelectList(db.Categories, "CategoryID", "CategoryName");
            ViewBag.SubjectID = subject1;

            return View(model1);
        }

        // GET: Question/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            questions questions = db.questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // POST: Question/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            questions questions = db.questions.Find(id);
            questions.Isdelete = true;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
