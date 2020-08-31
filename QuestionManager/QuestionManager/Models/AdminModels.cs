using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Web;

namespace QuestionManager.Models
{
    public class AdminModels
    {
        /// <summary>
        /// 課題の最後に行う全問題を出題するテストの名前「確認テスト」
        /// </summary>
        public const string FINAL_TEST_NAME = "確認テスト";

        /// <summary>
        /// 課題の最後のテストの制限時間
        /// </summary>
        public const int FINAL_TEST_TIMELIMIT = 20;

        /// <summary>
        /// ユーザー情報
        /// </summary>
        public class UserProfile
        {
            // ID(読み込むときに連番で振る)
            public int Sid { set; get; }

            // ユーザーID
            public string ID { set; get; }

            // メールアドレス
            public string Mail { set; get; }

            // 表示名
            public string Name { get; set; }

            // 凍結
            public bool IsLocked { get; set; }

            // 管理者権限
            public bool Admin { set; get; }
        }

        /// <summary>
        /// jqgridに渡すユーザー情報
        /// </summary>
        public class JQGrid
        {
            public int Total { get; set; }

            public int Page { get; set; }

            public int Records { get; set; }

            public Array Rows { get; set; }
        }


        /// <summary>
        /// カテゴリ情報
        /// </summary>
        public class CategoryList
        {
            // カテゴリ名
            public string CategoryName { get; set; }

            // カテゴリID
            public int CategoryID { get; set; }
        }

        /// <summary>
        /// 管理者用ホームの表示データ
        /// </summary>
        public class DisplayIndexData
        {
            // 受験者ID
            public Guid UserID { set; get; }

            // 受験者名
            public string UserName { set; get; }

            // 受験結果
            public List<AdminModels.ProgressList> ProgressLists { set; get; }
        }

        /// <summary>
        /// 受験結果
        /// </summary>
        public class ProgressList
        {
            // 課題ID
            public int SubjectID { set; get; }

            // 受験回数
            public int ChallengeTime { set; get; }

            // 合否
            public bool IsPass { set; get; }

            // 受験日時
            public DateTime TestDay { set; get; }
        }

        public class CategoryIndex
        {
            // 全カテゴリ
            public List<CategoryList> AllCategoryNameList { set; get; }

            // カテゴリー名
            public string CategoryName { set; get; }

            // カテゴリー内の課題数
            public int SubjectCount { set; get; }

            // 表示する課題名リスト
            public List<string> SubjectNames { set; get; }
        }

        // Indexに渡すmodelに全部詰め込むほうがいい
        public class DisplayAdminIndex
        {
            // 受験者のデータ
            public List<DisplayIndexData> UserData { set; get; }

            // カテゴリ情報
            public CategoryIndex Categories { set; get; }
        }

        // Admin/EditSubject に渡すデータ
        public class SubjectsData
        {
            // カテゴリ情報
            public List<Categories> Categories { set; get; }

            // 課題情報
            public List<subjects> Subjects { set; get; }
        }

        /// <summary>
        /// 課題受験回数
        /// </summary>
        public class Level
        {
            // 課題名
            [DisplayName("課題名")]
            public string SubjectName { get; set; }

            // 平均受験回数
            [DisplayName("平均受験回数")]
            public double AverageChallengeTimes { get; set; }
        }

        /// <summary>
        /// 課題到達度
        /// </summary>
        public class Progress
        {
            // 受験者
            public string UserName { get; set; }

            // 到達度(%)
            public decimal ChallengeProgress { get; set; }
        }

        /// <summary>
        /// 受験履歴
        /// </summary>
        public class ResultAllHistory
        {
            // ユーザーID
            public string UserID { set; get; }

            // 教科ID
            public int SubjectID { set; get; }

            // 解答回数
            public int ChallengeCount { set; get; }

            // 正解数
            public int CorrectAnswers { set; get; }

            // ユーザ名
            public string UserName { get; set; }

            // 教科名
            public string SubjectNames { set; get; }

            // 問題数
            public int QuestionCount { set; get; }

            // 受験日
            public DateTime? TestDay { get; set; }
        }

        /// <summary>
        /// 管理者用受験履歴画面の構成要素
        /// </summary>
        public class DisplayAdminSummary
        {
            // 管理者用受験履歴のリスト
            public List<ResultAllHistory> AdminSummaryList { set; get; }

            // カテゴリ情報のリスト
            public List<CategoryList> CategoriesList { set; get; }

            // ユーザー名
            public string UserName { set; get; }

            // ユーザーID
            public string UserID { set; get; }

            // カテゴリID
            public int? CategoryID { set; get; }

            // カテゴリ名
            public string CategoryName { set; get; }
        }

        /// <summary>
        /// 解答履歴
        /// </summary>
        public class DisplayAllHistory
        {
            // 教科ID
            public int SubjectID { get; set; }

            // ユーザー解答履歴
            public string UserAnswer { get; set; }

            // 問題文
            public string DisplayQuestion { get; set; }

            // 正否
            public bool IsSuccess { get; set; }

            // ユーザ名
            public string UserName { get; set; }
        }

        /// <summary>
        /// 管理者用解答履歴画面の構成要素
        /// </summary>
        public class DisplayAdminDetails
        {
            // 管理者用解答履歴のリスト
            public List<DisplayAllHistory> AdminDetailsList { get; set; }

            // ユーザー名
            public string UserName { get; set; }

            // ユーザーID
            public string UserID { get; set; }

            // 教科名
            public string SubjectName { get; set; }

            // 教科ID
            public int? SubjectID { get; set; }

            // 受験回数
            public int? ChallengeTime { get; set; }

            // カテゴリ名
            public string CategoryName { get; set; }

            // カテゴリID
            public int? CategoryID { get; set; }
        }

        /// <summary>
        /// 受験者選択
        /// </summary>
        public class DisplayNames
        {
            // カテゴリ情報のリスト
            public List<CategoryList> CategoriesList { set; get; }

            // 受験者ID
            public string UserID { set; get; }

            // 受験者名
            public string UserNames { set; get; }

            // 教科ID
            public int SubjectID { get; set; }
        }
    }
}