using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Web.Security;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
using QuestionManager.Models;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using QuestionManager.Controllers;
using Microsoft.AspNet.Identity.EntityFramework;

namespace QuestionManager.Models
{
    public class ResetPassword
    {
        /// <summary>
        /// パスワードのリセット
        /// </summary>
        /// <param name="mail">変更するアカウント</param>
        /// <returns></returns>
        public string Reset(string mail)
        {
            string pass = new PasswordHasher().HashPassword("password");

            using (var en = new UserEntities())
            {
                var user = en.AspNetUsers.Where(x => x.Email == mail).SingleOrDefault();

                if (user == null)
                {
                    // ユーザーが見つからない
                    return "アカウントが見つかりませんでした。";
                }

                user.PasswordHash = pass;
                en.SaveChanges();

                // ロックされていたら解除
                if (user.LockoutEnabled)
                {
                    user.LockoutEnabled = false;
                    en.SaveChanges();
                }
            }
            return mail + "さんの新しいパスワードは\"password\"です。";
        }
    }
}