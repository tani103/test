using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace QuestionManager.Models
{
    public class EditSubject
    {
        /// <summary>
        /// 課題名の取得
        /// </summary>
        /// <returns></returns>
        public List<subjects> GetSubject(int categoryID)
        {
            List<subjects> model = null;
            using (QuestionEntities db = new QuestionEntities())
            {
                // 課題名を読み込む
                model = db.subjects.Where(x => x.Isdelete == false && x.CategoryID == categoryID).Where(x => x.SubjectName != AdminModels.FINAL_TEST_NAME).OrderBy(x => x.SubjectOrder).ToList();
            }
            return model;
        }

        /// <summary>
        /// 課題名の削除
        /// </summary>
        /// <param name="subjectID">課題ID</param>
        public void DeleteSubject(int? subjectID)
        {
            using (QuestionEntities db = new QuestionEntities())
            {

                // 一致する課題を探す
                var subject = db.subjects.Where(x => x.SubjectID == subjectID).SingleOrDefault();
                if (subject != null)
                {
                    // 見つかれば削除
                    subject.Isdelete = true;

                    // 問題も消す
                    var question = db.questions.Where(x => x.SubjectID == subjectID);
                    foreach (var item in question)
                    {
                        item.Isdelete = true;
                    }

                    // progressも消す
                    var progress = db.MemberProgress.Where(x => x.SubjectID == subjectID);
                    foreach (var item in progress)
                    {
                        db.MemberProgress.Remove(item);
                    }

                    // temporaryProgress
                    var temporaryProgress = db.TemporaryProgress.Where(x => x.SubjectID == subjectID);
                    foreach (var item in temporaryProgress)
                    {
                        db.TemporaryProgress.Remove(item);
                    }

                    // Historys
                    var Historys = db.Historys.Where(x => x.SubjectID == subjectID);
                    foreach (var item in Historys)
                    {
                        db.Historys.Remove(item);
                    }

                    // examResult
                    var examResult = db.ExamResult.Where(x => x.SubjectID == subjectID);
                    foreach (var item in examResult)
                    {
                        db.ExamResult.Remove(item);
                    }
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// 課題名の追加
        /// </summary>
        /// <param name="name">追加する課題名</param>
        /// <returns>結果</returns>
        public string AddSubject(string name, int categoryID)
        {
            using (QuestionEntities db = new QuestionEntities())
            {
                var subject = db.subjects.Where(x => x.SubjectName == name).SingleOrDefault();
                // 同じ名前の課題名があって
                if (subject != null)
                {
                    // 削除フラグがある場合、DBから削除して、新しく登録する
                    if (subject.Isdelete == false)
                    {
                        // 削除フラグがなければ、新しく登録しない
                        return "同じ課題名が存在します。";
                    }

                    int subjectID = subject.SubjectID;

                    // 問題を消す
                    var q = db.questions.Where(x => x.SubjectID == subjectID);
                    foreach (var data in q)
                    {
                        db.questions.Remove(data);
                    }

                    db.SaveChanges();
                    db.subjects.Remove(subject);
                    db.SaveChanges();
                }

                // 追加する課題名のs_order(表示する順位)を一番下に設定
                int s_order = db.subjects.Max(x => x.SubjectID) + 1;
                db.subjects.Add(new subjects()
                {
                    //SubjectID = 0,
                    SubjectName = name,
                    SubjectOrder = s_order,
                    Isdelete = false,
                    CategoryID = categoryID,
                    TimeLimit = 10
                }
                );
                db.SaveChanges();

                return "課題名を登録しました。";
            }
        }

        /// <summary>
        /// 課題名の順位変え
        /// </summary>
        /// <param name="id">課題ID</param>
        /// <param name="direction">1なら順位を上げ、-1なら下げる</param>
        public void ChangeSOrder(int? id, int direction, int categoryID)
        {
            using (QuestionEntities db = new QuestionEntities())
            {
                // 課題リスト
                List<subjects> sub = db.subjects.Where(x => x.Isdelete == false && x.CategoryID == categoryID).OrderBy(x => x.SubjectOrder).ToList();
                // 入れ替え元
                var s1 = db.subjects.Where(x => x.SubjectID == id).SingleOrDefault();

                // 入れ替え先課題ID
                int changeid = 0;
                // 課題リストから入れ替え先の課題を探す
                for (int i = 0; i < sub.Count; i++)
                {
                    if (sub[i].SubjectID == id)
                    {
                        changeid = sub[i - direction].SubjectID;
                        break;
                    }
                }
                // 入れ替え先
                var s2 = db.subjects.Where(x => x.SubjectID == changeid).SingleOrDefault();

                // 入れ替え
                int j = s2.SubjectOrder;
                s2.SubjectOrder = s1.SubjectOrder;
                s1.SubjectOrder = j;
                db.SaveChanges();
            }
        }

        /// <summary>
        /// 課題名の変更
        /// </summary>
        /// <param name="id">課題ID</param>
        /// <param name="changeSubject">変更する課題名</param>
        public string ChangeSubject(int? id, string changeSubject, int categoryID)
        {
            using (QuestionEntities db = new QuestionEntities())
            {
                var subjects = db.subjects.Where(x => x.CategoryID == categoryID).Where(x => x.SubjectName == changeSubject);

                foreach (var subject in subjects)
                {
                    // 同じ名前の課題名があって
                    if (subject != null)
                    {
                        // 削除フラグがある場合、DBから削除して、新しく登録する
                        if (subject.Isdelete == false)
                        {
                            // 削除フラグがなければ、新しく登録しない
                            return "同じ課題名が存在します。";
                        }

                        int subjectID = subject.SubjectID;

                        // 問題を消す
                        var q = db.questions.Where(x => x.SubjectID == subjectID);
                        foreach (var data in q)
                        {
                            db.questions.Remove(data);
                        }

                        db.SaveChanges();
                        db.subjects.Remove(subject);
                    }
                }
                db.SaveChanges();

                // 変更する課題データ
                var subject2 = db.subjects.Where(x => x.SubjectID == id).First();
                // 変更
                if (subject2.SubjectName != changeSubject)
                {
                    subject2.SubjectName = changeSubject;
                }
                db.SaveChanges();

                return null;
            }
        }

        /// <summary>
        /// 課題の1日受験回数制限の変更
        /// </summary>
        /// <returns>結果</returns>
        public string LimitExam()
        {
            string limit = "false";
            if (ConfigurationManager.AppSettings["LimitExam"] != "true")
            {
                limit = "true";
            }

            // 受験回数制限の変更
            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings["LimitExam"].Value = limit;
            config.Save();

            return "受験回数制限の有無を変更しました。";
        }

        /// <summary>
        /// 課題の合格点の設定
        /// </summary>
        /// <param name="value">合格点</param>
        /// <returns>結果</returns>
        public string PassingMark(string value)
        {
            int r = 0;
            if (!Int32.TryParse(value, out r) || r < 0 || 100 < r)
            {
                return "点数を入力してください。";
            }

            // 合格点の変更
            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings["PassingMark"].Value = value;
            config.Save();

            return "課題の合格点を変更しました。";
        }

        /// <summary>
        /// 確認テストの合格点の設定
        /// </summary>
        /// <param name="value">合格点</param>
        /// <returns>結果</returns>
        public string FinalPassingMark(string value)
        {
            int r = 0;
            if (!Int32.TryParse(value, out r) || r < 0 || 100 < r)
            {
                return "点数を入力してください。";
            }

            // 合格点の変更
            var config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            config.AppSettings.Settings["FinalPassingMark"].Value = value;
            config.Save();

            return "確認テストの合格点を変更しました。";
        }

        /// <summary>
        /// 課題名の変更
        /// </summary>
        /// <param name="id">課題ID</param>
        /// <param name="changeSubject">変更する課題名</param>
        public void SetTime(int? id, string changeSubject)
        {
            using (QuestionEntities db = new QuestionEntities())
            {


                // 変更する課題データ
                var subject = db.subjects.Where(x => x.SubjectID == id).First();
                // 変更
                if (subject.SubjectName != changeSubject)
                {
                    subject.SubjectName = changeSubject;
                }
                db.SaveChanges();
            }
        }


    }
}