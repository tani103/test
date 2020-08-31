using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace QuestionManager.Models
{
    public class EditCategories
    {
        /// <summary>
        /// カテゴリ一覧取得
        /// </summary>
        /// <returns></returns>
        public List<Categories> GetCategories()
        {
            List<Categories> model = null;

            using (QuestionEntities db = new QuestionEntities())
            {
                model = db.Categories.Where(x => x.IsDelete == false).ToList();
            }
            return model;
        }

        /// <summary>
        /// カテゴリ登録
        /// </summary>
        /// <param name="newCategory"></param>
        /// <param name="categoryID"></param>
        /// <returns></returns>
        public string AddCategory(string newCategory)
        {
            // エラーある
            using (QuestionEntities que = new QuestionEntities())
            {
                var category = que.Categories.Where(x => x.CategoryName == newCategory).SingleOrDefault();
                if (category != null)
                {
                    if (category.IsDelete == false)
                    {
                        // 削除フラグが無ければ登録しない
                        return "同じカテゴリ名が存在します。";
                    }

                    // 削除フラグがある同じ名前のカテゴリがあるとき
                    int categoryID = category.CategoryID;
                    var subject = que.subjects.Where(x => x.CategoryID == categoryID);
                    foreach (var item in subject)
                    {
                        que.subjects.Remove(item);
                    }

                    que.Categories.Remove(category);
                    que.SaveChanges();
                }

                // カテゴリ登録

                que.Categories.Add(new Categories()
                {
                    CategoryName = newCategory,
                    IsDelete = false
                });


                que.SaveChanges();

                // 確認テスト登録
                var sub = que.subjects;
                int s_order = 0;

                if (0 < sub.Count())
                {
                    var max = sub.Max(x => x.SubjectID) + 1;
                }
                else
                {
                    s_order = 1;
                }

                que.subjects.Add(new subjects()
                {
                    SubjectName = AdminModels.FINAL_TEST_NAME,
                    SubjectOrder = s_order,
                    Isdelete = false,
                    CategoryID = que.Categories.Max(x => x.CategoryID),
                    TimeLimit = AdminModels.FINAL_TEST_TIMELIMIT
                });
                que.SaveChanges();

                return "カテゴリを登録しました。";
            }
        }

        /// <summary>
        /// カテゴリの削除
        /// </summary>
        /// <param name="categoryID"></param>
        public void DeleteCategory(int? categoryID)
        {
            using (QuestionEntities que = new QuestionEntities())
            {
                // カテゴリ取得
                var category = que.Categories.Where(x => x.CategoryID == categoryID).SingleOrDefault();
                if (category != null)
                {
                    category.IsDelete = true;
                }

                // カテゴリに含まれている教科も削除
                var subjects = que.subjects.Where(x => x.CategoryID == categoryID);
                if (subjects != null)
                {
                    foreach (var subject in subjects)
                    {
                        subject.Isdelete = true;
                        int subjectID = subject.SubjectID;

                        // 問題も消す
                        var question = que.questions.Where(x => x.SubjectID == subjectID);
                        foreach (var item in question)
                        {
                            item.Isdelete = true;
                        }

                        // progressも消す
                        var progress = que.MemberProgress.Where(x => x.SubjectID == subjectID);
                        foreach (var item in progress)
                        {
                            que.MemberProgress.Remove(item);
                        }

                        // temporaryProgress
                        var temporaryProgress = que.TemporaryProgress.Where(x => x.SubjectID == subjectID);
                        foreach (var data in temporaryProgress)
                        {
                            que.TemporaryProgress.Remove(data);
                        }

                        // Historys
                        var Historys = que.Historys.Where(x => x.SubjectID == subjectID);
                        foreach (var data in Historys)
                        {
                            que.Historys.Remove(data);
                        }

                        // examResult
                        var examResult = que.ExamResult.Where(x => x.SubjectID == subjectID);
                        foreach (var data in examResult)
                        {
                            que.ExamResult.Remove(data);
                        }
                    }
                }

                que.SaveChanges();
            }
        }

        /// <summary>
        /// カテゴリ名の変更
        /// </summary>
        /// <param name="categoryID">変更するカテゴリID</param>
        /// <param name="changeCategory">変更するカテゴリ名</param>
        public void ChangeCategory(int? categoryID, string changeCategory)
        {
            using (QuestionEntities que = new QuestionEntities())
            {
                var category = que.Categories.Where(x => x.CategoryID == categoryID).First();

                if (category.CategoryName != changeCategory)
                {
                    category.CategoryName = changeCategory;
                }
                que.SaveChanges();
            }
        }
    }
}