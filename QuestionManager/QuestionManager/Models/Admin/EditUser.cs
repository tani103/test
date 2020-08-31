using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Profile;
using System.Web.Mvc;
using QuestionManager.Controllers;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using QuestionManager.Models;



namespace QuestionManager.Models
{
    public class EditUser
    {

        /// <summary>
        /// ユーザー情報一覧取得
        /// </summary>
        /// <returns>ユーザー情報一覧</returns>
        public AdminModels.JQGrid GetUser()
        {
            var result = new AdminModels.JQGrid();
            List<AdminModels.UserProfile> list = new List<AdminModels.UserProfile>();

            int id = 1;
            using (UserEntities userDB = new UserEntities())
            {
                var users = userDB.AspNetUsers.OrderBy(x => x.CreateDate);
                foreach (var user in users)
                {
                    AdminModels.UserProfile data = new AdminModels.UserProfile();
                    data.Sid = id++;
                    data.ID = user.Id.ToString();
                    data.Mail = user.Email;
                    data.IsLocked = user.LockoutEnabled;
                    data.Admin = false;

                    var hasRole = user.AspNetRoles.Where(x => x.Name == "Admin").SingleOrDefault();
                    if (hasRole != null)
                    {
                        data.Admin = true;
                    }



                    data.Name = user.UserName;
                    list.Add(data);
                    string s = user.Email;
                }
            }

            // 登録されたユーザーを取得
            try
            {
            }
            catch
            {
                // データがなければ空のリストを渡す
                return result;
            }

            // listをjsonに変換
            result.Total = 1;
            result.Page = 1;
            result.Records = list.Count;
            result.Rows = list.ToArray();
            return result;
        }

        /// <summary>
        /// ユーザー情報の編集
        /// </summary>
        /// <param name="data">編集するユーザーの情報</param>
        public string EditUserData(FormCollection data)
        {
            string id = data["ID"];

            // ユーザー情報
            using (UserEntities users = new UserEntities())
            {
                // 現在のユーザー情報
                var user = users.AspNetUsers.Where(x => x.Id == id).SingleOrDefault();
                if (user == null)
                {
                    // DBにユーザー情報が見つからない場合
                    return "ユーザー情報が見つかりません。";
                }

                foreach (string s in data.AllKeys)
                {
                    // 変更された情報を探す
                    switch (s)
                    {
                        case "Name":

                            // 表示名変更処理
                            user.UserName = data["Name"];
                            users.SaveChanges();

                            return null;
                        case "Mail":

                            string newMail = data["Mail"];

                            // 変更処理
                            if (!users.AspNetUsers.Any(x => x.Email == newMail))
                            {
                                // ログインアドレス変更
                                user.Email = data["Mail"];
                                users.SaveChanges();
                            }
                            // 変更先のアドレスが既に存在する場合
                            else
                            {
                                return "既に登録されているメールアドレスです。";
                            }
                            return null;
                        case "Admin":
                            // 管理者権限の付与・剥奪
                            // ユーザーが同じロール(管理者)を2つ以上持たないこととする
                            var roleAdmin = users.AspNetRoles.Where(x => x.Name == "Admin").SingleOrDefault();
                            var checkAdmin = user.AspNetRoles.Where(x => x.Name == "Admin").SingleOrDefault();

                            if (checkAdmin != null)
                            {
                                // 管理者権限を奪う
                                if (data["Admin"] == "false")
                                {
                                    user.AspNetRoles.Remove(roleAdmin);
                                }
                            }
                            else
                            {
                                // 管理者権限を与える
                                if (data["Admin"] == "true")
                                {
                                    user.AspNetRoles.Add(roleAdmin);
                                }
                            }
                            users.SaveChanges();
                            return null;
                        case "IsLocked":
                            // 凍結/凍結解除
                            string next = data["IsLocked"];
                            if (user.LockoutEnabled && next == "false")
                            {
                                user.LockoutEnabled = false;
                            }
                            else if (!user.LockoutEnabled && next == "true")
                            {
                                user.LockoutEnabled = true;
                            }
                            users.SaveChanges();
                            return null;
                        default:
                            return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ユーザーを登録する
        /// </summary>
        /// <param name="model">登録するユーザー情報</param>
        /// <returns>エラーメッセージ</returns>
        public string InsertUserData(FormCollection data)
        {
            using (var ue = new UserEntities())
            {
                string mail = data["Mail"];

                // 既に存在するメールアドレスならメッセージボックス表示
                if (ue.AspNetUsers.Where(x => x.Email == mail).Count() != 0)
                {
                    return "既に登録されているメールアドレスです。";
                }

                // ユーザー追加
                string pass = new PasswordHasher().HashPassword("password");
                var user = new ApplicationUser { UserName = data["Name"], Email = data["Mail"] };

                ue.AspNetUsers.Add(new AspNetUsers()
                {
                    Id = user.Id.ToString(),
                    Email = mail,
                    EmailConfirmed = false,
                    PasswordHash = pass,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = null,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEndDateUtc = null,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    UserName = data["Name"],
                    CreateDate = DateTime.Now
                });

                ue.SaveChanges();

                // 管理者は、ユーザーのロールと管理者のロールの両方を持つ
                var addedUser = ue.AspNetUsers.Where(x => x.Email == mail).SingleOrDefault();

                var userRole = ue.AspNetRoles.Where(x => x.Name == "User").SingleOrDefault();
                var adminRole = ue.AspNetRoles.Where(x => x.Name == "Admin").SingleOrDefault();

                addedUser.AspNetRoles.Add(userRole);

                // 管理者権限
                if (data["Admin"] == "true")
                {
                    // 管理者権限追加
                    addedUser.AspNetRoles.Add(adminRole);
                }

                ue.SaveChanges();

            }
            return null;
        }

        /// <summary>
        /// ユーザーを削除する
        /// </summary>
        /// <param name="model">登録するユーザー情報</param>
        /// <returns>エラーメッセージ</returns>
        public string DeleteUserData(FormCollection data)
        {
            string[] dataArray = data["ID"].Split(',');

            List<Guid> id = new List<Guid>();
            List<string> userID = new List<string>();

            foreach (string inputData in dataArray)
            {
                // ユーザーID
                id.Add(Guid.Parse(inputData));
                userID.Add(inputData);
                // ユーザー名
            }

            using (UserEntities users = new UserEntities())
            {
                foreach (var inputID in id)
                {
                    var u = users.AspNetUsers.Where(x => x.Id == inputID.ToString()).FirstOrDefault();
                    // 削除
                    if (u != null)
                    {
                        users.AspNetUsers.Remove(u);
                    }
                }

                users.SaveChanges();
            }

            using (QuestionEntities db = new QuestionEntities())
            {
                foreach (var inputID in id)
                {
                    // ユーザーのmember_progressを削除
                    var progress = db.MemberProgress.Where(x => x.UserID == inputID).FirstOrDefault();

                    if (progress != null)
                    {
                        db.MemberProgress.Remove(progress);
                    }
                }

                foreach (var inputUserID in userID)
                {
                    // ユーザーのtemporaryProgressを削除
                    var temporaryProgress = db.TemporaryProgress.Where(x => x.UserID == inputUserID).FirstOrDefault();

                    if (temporaryProgress != null)
                    {
                        db.TemporaryProgress.Remove(temporaryProgress);
                    }
                }

                foreach (var inputUserID in userID)
                {
                    // ユーザーのHistorysを削除
                    var Historys = db.Historys.Where(x => x.UserID == inputUserID).FirstOrDefault();

                    if (Historys != null)
                    {
                        db.Historys.Remove(Historys);
                    }
                }

                foreach (var inputUserID in userID)
                {
                    // ユーザーのexamResultを削除
                    var examResult = db.ExamResult.Where(x => x.UserID == inputUserID).FirstOrDefault();

                    if (examResult != null)
                    {
                        db.ExamResult.Remove(examResult);
                    }
                }

                db.SaveChanges();
            }

            return null;
        }
    }
}