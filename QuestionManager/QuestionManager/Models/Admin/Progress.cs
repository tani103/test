using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;

namespace QuestionManager.Models
{
    public class Progress
    {
        /// <summary>
        /// 課題到達度を表示
        /// </summary>
        /// <param name="categoryID">カテゴリID</param>
        /// <param name="firstYear">検索開始年</param>
        /// <param name="finalYear">検索終了年</param>
        /// <returns></returns>
        public List<AdminModels.Progress> MakeList(int categoryID, int firstYear, int finalYear)
        {
            List<AdminModels.Progress> list = new List<AdminModels.Progress>();

            using (var ue = new UserEntities())
            {
                var user = ue.AspNetUsers.Where(x => x.CreateDate.Year >= firstYear && x.CreateDate.Year <= finalYear);

                using (var qe = new QuestionEntities())
                {
                    int count = qe.subjects.Where(x => x.CategoryID == categoryID).Where(x => x.Isdelete == false).Count() + 1;

                    foreach (var data in user)
                    {
                        int passCount = qe.MemberProgress.Where(x => x.UserID.ToString() == data.Id).Where(x => x.IsPass == true).Count();

                        string userName = data.UserName;
                        decimal score = (decimal)passCount / (decimal)count * 100;

                        AdminModels.Progress pro = new AdminModels.Progress();

                        pro.UserName = userName;
                        pro.ChallengeProgress = Math.Round(score);

                        list.Add(pro);
                    }
                }
            }

            list = list.OrderByDescending(x => x.ChallengeProgress).ToList();
            return list;
        }
    }
}