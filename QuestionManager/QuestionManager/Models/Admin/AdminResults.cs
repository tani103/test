using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuestionManager.Models;
using System.Diagnostics;
using System.Web.Mvc;
using System.ComponentModel;

namespace QuestionManager.Models
{
    /// <summary>
    /// 受験者履歴管理
    /// </summary>
    public class AdminResults
    {
        /// <summary>
        /// 受験者一覧を取得
        /// </summary>
        /// <returns></returns>
        public List<AdminModels.DisplayNames> GetDisplayUsers()
        {
            List<AdminModels.DisplayNames> model = new List<AdminModels.DisplayNames>();

            using (QuestionEntities que = new QuestionEntities())
            {
                using (UserEntities use = new UserEntities())
                {
                    var users = use.AspNetUsers.OrderBy(x => x.CreateDate);

                    foreach (var data in users)
                    {
                        string userId = data.Id;
                        string userName = data.UserName;

                        AdminModels.DisplayNames names = new AdminModels.DisplayNames();

                        names.UserID = userId;
                        names.UserNames = userName;
                        model.Add(names);
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// 各ユーザーの受験履歴を取得
        /// </summary>
        /// <param name="userID">ユーザーID</param>
        /// <returns>受験履歴</returns>
        public List<AdminModels.ResultAllHistory> GetAdminSummary(string userID, int? categoryID)
        {
            List<AdminModels.ResultAllHistory> list = new List<AdminModels.ResultAllHistory>();

            using (UserEntities use = new UserEntities())
            {
                using (QuestionEntities que = new QuestionEntities())
                {
                    var ques = que.ExamResult.Where(x => x.UserID == userID);
                    var sub = que.subjects.Where(x => x.CategoryID == categoryID);
                    var user = use.AspNetUsers;

                    foreach (var data in ques)
                    {
                        AdminModels.ResultAllHistory his = new AdminModels.ResultAllHistory();
                        int challengeCount = data.ChallengeTime;
                        int answer = data.CorrectAnswersCount;
                        int subjectId = data.SubjectID;
                        int questionCount = que.questions.Where(x => x.SubjectID == data.SubjectID).Where(x => x.Isdelete == false).Count();
                        DateTime? testDay = que.Historys.Where(x => x.SubjectID == data.SubjectID).Where(x => x.UserID == data.UserID).Where(x => x.ChallengeTime == data.ChallengeTime).Max(x => x.TestDay);
                        string subjectName = sub.Where(x => x.SubjectID == data.SubjectID).SingleOrDefault().SubjectName;
                        string userName = user.Where(x => x.Id.ToString() == data.UserID).SingleOrDefault().UserName;

                        his.UserID = data.UserID;
                        his.ChallengeCount = challengeCount;
                        his.CorrectAnswers = answer;
                        his.SubjectID = subjectId;
                        his.UserName = userName;
                        his.SubjectNames = subjectName;
                        his.QuestionCount = questionCount;
                        his.TestDay = testDay;

                        list.Add(his);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解答履歴を取得
        /// </summary>
        /// <param name="userID">ユーザーID</param>
        /// <param name="subjectID">教科ID</param>
        /// <param name="challengeTimes">受験回数</param>
        /// <returns>解答履歴</returns>
        public List<AdminModels.DisplayAllHistory> GetAdminDetail(string userID, int? subjectID, int? challengeTimes, int? categoryID)
        {
            List<AdminModels.DisplayAllHistory> model = new List<AdminModels.DisplayAllHistory>();

            using (QuestionEntities que = new QuestionEntities())
            {
                using (UserEntities use = new UserEntities())
                {
                    // 解答履歴をモデルに反映
                    var his = que.Historys.Where(x => x.UserID == userID).Where(x => x.ChallengeTime == challengeTimes).Where(x => x.SubjectID == subjectID);
                    var sub = que.subjects.Where(x => x.SubjectID == subjectID).Where(x => x.CategoryID == categoryID);
                    var ques = que.questions;

                    foreach (var data in his)
                    {
                        string userAnswer = data.UserAnswer;
                        int subjectId = data.SubjectID;
                        string userId = data.UserID;
                        bool isCorrect = data.IsCorrect;
                        string displayQuestion = que.questions.Where(x => x.Isdelete == false).Where(x => x.QuestionID == data.QuestionID).SingleOrDefault().question;

                        AdminModels.DisplayAllHistory dis = new AdminModels.DisplayAllHistory();
                        dis.UserAnswer = userAnswer;
                        dis.SubjectID = subjectId;
                        dis.IsSuccess = isCorrect;
                        dis.DisplayQuestion = displayQuestion;
                        model.Add(dis);

                    }
                }
            }

            return model;
        }

        /// <summary>
        /// カテゴリ情報取得
        /// </summary>
        /// <returns></returns>
        public List<AdminModels.CategoryList> GetAdminCategory()
        {
            List<AdminModels.CategoryList> model = new List<AdminModels.CategoryList>();
            using (QuestionEntities en = new QuestionEntities())
            {
                var que = en.Categories.Where(x => x.IsDelete == false).OrderBy(x => x.CategoryID);

                foreach (var item in que)
                {
                    string categoryName = item.CategoryName;
                    int categoryId = item.CategoryID;
                    AdminModels.CategoryList list = new AdminModels.CategoryList();
                    list.CategoryName = categoryName;
                    list.CategoryID = categoryId;
                    model.Add(list);
                }
            }

            return model;
        }
    }
}