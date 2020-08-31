using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace QuestionManager.Models
{

    public partial class Categories
    {
        public static Categories CreateCategories(global::System.Int32 categoryID, global::System.String categoryName, global::System.Boolean isDelete)
        {
            Categories categories = new Categories();
            categories.CategoryID = categoryID;
            categories.CategoryName = categoryName;
            categories.IsDelete = isDelete;
            return categories;

        }
    }
}