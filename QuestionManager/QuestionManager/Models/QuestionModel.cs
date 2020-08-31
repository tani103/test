using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace QuestionManager.Models
{
    public partial class questions
    {
        public int QuestionID { get; set; }
        public int SubjectID { get; set; }

        [Required]
        public string question { get; set; }

        [Required]
        public string Answer { get; set; }
        public bool Isdelete { get; set; }

        public virtual subjects subjects { get; set; }

        public string PicturePath;

        public SelectList SubjectSelectList { get; set; }

        public SelectList CategorySelectList { get; set; }
    }

    public partial class edit
    {
        public int QuestionID { get; set; }
        public int SubjectID { get; set; }
        public string question { get; set; }
        public string Answer { get; set; }
        public bool Isdelete { get; set; }

        public virtual subjects subjects { get; set; }

        public string PicturePath;

        public SelectList SubjectSelectList { get; set; }

        public SelectList CategorySelectList { get; set; }
    }

    /// <summary>
    /// カテゴリ名のリスト（問題編集時にドロップダウンで選択させる）
    /// </summary>
    public class CategoriesList
    {
        // カテゴリID
        public int CategoryID { get; set; }

        // カテゴリ名
        public string CategoryName { get; set; }

        // 課題名のリスト
        public IEnumerable<SubjectList> SubjectList { get; set; }

        // セレクトボックス変化用
        public IEnumerable<CategoriesList> CreateDefaultsCategory()
        {
            var clist = new List<CategoriesList>();

            using (QuestionEntities db = new QuestionEntities())
            {
                var c = db.Categories.Where(x => x.IsDelete == false).ToList();

                foreach (var cItem in c)
                {
                    var slist = new List<SubjectList>();
                    var s = db.subjects.Where(x => x.Isdelete == false && x.CategoryID == cItem.CategoryID).ToList();

                    foreach (var sItem in s)
                    {
                        slist.Add(new SubjectList { SubjectID = sItem.SubjectID, SubjectName = sItem.SubjectName });
                    }

                    clist.Add(new CategoriesList { CategoryID = cItem.CategoryID, CategoryName = cItem.CategoryName, SubjectList = slist });
                }
            }
            return clist;
        }
    }

    /// <summary>
    /// 課題名のリスト（問題編集時にドロップダウンで選択させる）
    /// </summary>
    public class SubjectList
    {
        // 教科ID
        public int SubjectID { set; get; }

        // 課題名
        public string SubjectName { set; get; }

        // カテゴリID
        public int CategoryID { get; set; }
    }


    public class index
    {
        public virtual subjects subjects { get; set; }

        public string question { get; set; }

        public string SubjectName { get; set; }

        public string Answer { get; set; }

        public int QuestionID { get; set; }

    }

    public class create
    {
        // 問題ID
        public int QuestionID { get; set; }
        // 課題名
        public int SubjectID { get; set; }
        // 問題
        public string question { get; set; }
        // 解答
        public string Answer { get; set; }
        // 削除
        public bool Isdelete { get; set; }

        public virtual subjects subjects { get; set; }

        // 画像のファイルパス
        public string PicturePath;
    }
    public class drop
    {
        public string ProductID { get; set; }
    }
}