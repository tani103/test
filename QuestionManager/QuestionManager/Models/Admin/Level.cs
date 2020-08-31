using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace QuestionManager.Models
{
    public class Level
    {
        /// <summary>
        /// 課題受験回数を表示
        /// </summary>
        /// <param name="categoryID">カテゴリID</param>
        /// <param name="firstYear">検索開始年</param>
        /// <param name="finalYear">検索終了年</param>
        /// <returns></returns>
        public List<AdminModels.Level> MakeList(int categoryID, int firstYear, int finalYear)
        {
            // 表示する受験者取得
            List<AspNetUsers> users = new List<AspNetUsers>();
            List<AdminModels.Level> list = new List<AdminModels.Level>();
            using (UserEntities use = new UserEntities())
            {
                using (QuestionEntities que = new QuestionEntities())
                {
                    var pro = que.MemberProgress;
                    var user = use.AspNetUsers.Where(x => x.CreateDate.Year >= firstYear && x.CreateDate.Year <= finalYear);
                    var sub = que.subjects.Where(x => x.Isdelete == false).Where(x => x.CategoryID == categoryID).OrderBy(x => x.SubjectOrder);

                    AdminModels.Level final = new AdminModels.Level();

                    foreach (var data in sub)
                    {
                        int count = 0;
                        var member = pro.Where(x => x.SubjectID == data.SubjectID).Where(x => x.IsPass == true);
                        foreach (var item in member)
                        {
                            foreach (var u in user)
                            {
                                if (item.UserID.ToString() == u.Id)
                                {
                                    int times = item.ChallengeTimes;
                                    count += times;
                                }
                            }
                        }
                        string subjectName = data.SubjectName;
                        double counts = 0;
                        if (member.Count() != 0)
                        {
                            counts = count / member.Count();
                        }

                        if (subjectName != "確認テスト")
                        {
                            AdminModels.Level level = new AdminModels.Level();
                            level.SubjectName = subjectName;
                            level.AverageChallengeTimes = counts;
                            list.Add(level);
                        }
                        else
                        {
                            final.SubjectName = subjectName;
                            final.AverageChallengeTimes = counts;
                        }
                    }

                    list.Add(final);
                }
            }

            return list;
        }
    }
}