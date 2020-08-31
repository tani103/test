using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuestionManager.Models;

namespace QuestionManager.Models
{
    public class AdminIndex
    {
        public List<AdminModels.DisplayIndexData> GetIndexData(int categoryID)
        {
            List<AdminModels.DisplayIndexData> list = new List<AdminModels.DisplayIndexData>();

            using (QuestionEntities que = new QuestionEntities())
            {
                var mem = que.MemberProgress.Where(x => x.subjects.CategoryID == categoryID);

                // MemberProgressの確認テストに合格しているユーザーを取得
                var final = que.subjects.Where(x => x.CategoryID == categoryID).Where(x => x.SubjectName == AdminModels.FINAL_TEST_NAME).SingleOrDefault();
                int? finalID = null;

                if (final != null)
                {
                    finalID = final.SubjectID;
                }

                var passedUsers = mem.Where(x => x.SubjectID == finalID).Where(x => x.IsPass == true);

                // MemberProgressにデータがあるユーザーで、確認テストを合格していないユーザー
                List<Guid> usersList = new List<Guid>();
                foreach (var u in mem)
                {
                    usersList.Add(u.UserID);
                }

                foreach (var u in passedUsers)
                {
                    usersList.Remove(u.UserID);
                }

                var usersList2 = usersList.Distinct();

                using (UserEntities ue = new UserEntities())
                {
                    foreach (var user in usersList2)
                    {
                        AdminModels.DisplayIndexData data = new AdminModels.DisplayIndexData();

                        var userName = ue.AspNetUsers.Where(x => x.Id == user.ToString()).SingleOrDefault().UserName;

                        data.UserID = user;
                        data.UserName = userName;
                        data.ProgressLists = GetProgressLists(user.ToString(), categoryID);

                        list.Add(data);
                    }
                }
            }

            return list;
        }

        public List<AdminModels.ProgressList> GetProgressLists(string userID, int categoryID)
        {

            List<AdminModels.ProgressList> list = new List<AdminModels.ProgressList>();

            using (QuestionEntities que = new QuestionEntities())
            {
                // カテゴリ内の課題数を取得
                var sub = que.subjects;

                var order = sub.Where(x => x.CategoryID == categoryID).Where(x => x.Isdelete == false).OrderBy(x => x.SubjectOrder);
                var mem = que.MemberProgress.Where(x => x.UserID.ToString() == userID);

                AdminModels.ProgressList final = new AdminModels.ProgressList();

                foreach (var data in order)
                {
                    // 確認テストの結果をMemberProgressに格納してなかったらだめかもしれない


                    var prog = mem.Where(x => x.SubjectID == data.SubjectID).SingleOrDefault();
                    AdminModels.ProgressList tmp = new AdminModels.ProgressList();

                    if (prog != null)
                    {
                        var his = que.Historys.Where(x => x.UserID == userID).Where(x => x.SubjectID == prog.SubjectID).OrderByDescending(x => x.ChallengeTime).FirstOrDefault();

                        tmp.SubjectID = data.SubjectID;
                        tmp.ChallengeTime = prog.ChallengeTimes;
                        tmp.IsPass = (bool)prog.IsPass;
                        tmp.TestDay = his.TestDay;
                    }
                    else
                    {
                        tmp.SubjectID = data.SubjectID;
                        tmp.ChallengeTime = 0;
                        tmp.IsPass = false;
                        tmp.TestDay = DateTime.Now;

                    }

                    if (data.SubjectName != AdminModels.FINAL_TEST_NAME)
                    {
                        list.Add(tmp);
                    }
                    else
                    {
                        final = tmp;
                    }
                }

                list.Add(final);

                return list;
            }
        }

        public AdminModels.CategoryIndex GetCategoryIndex(int categoryID)
        {
            var data = new AdminModels.CategoryIndex();

            using (QuestionEntities que = new QuestionEntities())
            {
                var cat = que.Categories.Where(x => x.IsDelete != true).OrderBy(x => x.CategoryID).Select(x => new { x.CategoryName, x.CategoryID });
                List<AdminModels.CategoryList> categoryLists = new List<AdminModels.CategoryList>();

                foreach (var c in cat)
                {
                    AdminModels.CategoryList cl = new AdminModels.CategoryList();
                    cl.CategoryName = c.CategoryName;
                    cl.CategoryID = c.CategoryID;

                    categoryLists.Add(cl);
                }

                data.AllCategoryNameList = categoryLists;

                data.CategoryName = que.Categories.Where(x => x.CategoryID == categoryID).Single().CategoryName;
                data.SubjectCount = que.subjects.Count(x => x.CategoryID == categoryID);

                List<string> names = new List<string>();
                var tmp = que.subjects.Where(x => x.CategoryID == categoryID).Where(x => x.Isdelete == false).OrderBy(x => x.SubjectOrder);

                foreach (var name in tmp)
                {
                    if (name.SubjectName != AdminModels.FINAL_TEST_NAME)
                    {
                        names.Add(name.SubjectName);
                    }
                }

                names.Add(AdminModels.FINAL_TEST_NAME);

                data.SubjectNames = names;
            }

            return data;
        }

        public AdminModels.DisplayAdminIndex GetAllIndexData(int categoryID)
        {
            AdminModels.DisplayAdminIndex data = new AdminModels.DisplayAdminIndex();

            data.UserData = GetIndexData(categoryID);

            if (data.UserData.Count() == 0)
            {
                return null;
            }

            data.Categories = GetCategoryIndex(categoryID);

            return data;
        }
    }
}