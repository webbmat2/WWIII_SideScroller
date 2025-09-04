using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class WelcomeWindow : EditorWindow
{

    private static bool? _showOnStartup;

    public static bool ShowOnStartup
    {
        get
        {
            if (!_showOnStartup.HasValue)
            {
                _showOnStartup = EditorPrefs.GetBool("Bayat.PGAU.WelcomeWindow.ShowOnStartUp", true);
            }
            return _showOnStartup.GetValueOrDefault();
        }
        set
        {
            _showOnStartup = value;
        }
    }

    public abstract class Page
    {

        public abstract void OnPageGUI();

    }

    public class SplashPage : Page
    {

        protected readonly Texture splashTexture;

        public SplashPage(Texture splashTexture)
        {
            this.splashTexture = splashTexture;
        }

        public override void OnPageGUI()
        {
            GUILayout.Label(this.splashTexture, GUILayout.Width(960), GUILayout.Height(640));
        }

    }

    public class TextPage : Page
    {

        protected readonly string title;
        protected readonly string text;
        private Vector2 scrollPosition;

        public TextPage(string title, string text)
        {
            this.title = title;
            this.text = text;
        }

        public override void OnPageGUI()
        {
            EditorGUILayout.BeginScrollView(this.scrollPosition);
            GUILayout.Label(this.title, Styles.CenteredLargeLabel);
            GUILayout.Label(this.text, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }

    }

    public class ContactPage : Page
    {

        private Texture logo;

        public ContactPage(Texture logo)
        {
            this.logo = logo;
        }

        public override void OnPageGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(this.logo, GUILayout.Width(600));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("Thank you for choosing <b>Platform Game Assets Ultimate</b>", Styles.CenteredLargeLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Email"))
            {
                Application.OpenURL("mailto:support@bayat.io");
            }
            if (GUILayout.Button("Discord"))
            {
                Application.OpenURL("https://discord.gg/HWMqD7T");
            }
            if (GUILayout.Button("Forum"))
            {
                Application.OpenURL("https://forum.unity.com/threads/bayat-save-system-an-ultimate-data-management-solution.817416/");
            }
            if (GUILayout.Button("YouTube"))
            {
                Application.OpenURL("https://www.youtube.com/channel/UCDLJbvqDKJyBKU2E8TMEQpQ/");
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Label("Have an issue or question? Let us know!", EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndVertical();
        }

    }

    private int currentPageIndex = 0;
    private Page[] pages;

    private Texture[] splashTextures;
    private Texture logoTexture;

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        if (!ShowOnStartup)
        {
            return;
        }
        EditorApplication.update += Initialize;
    }

    [MenuItem("Window/Bayat/PGAU/Welcome")]
    private static void Initialize()
    {
        EditorApplication.update -= Initialize;
        var window = EditorWindow.GetWindow<WelcomeWindow>();
        window.titleContent = new GUIContent("Welcome");
        window.minSize = new Vector2(960, 700);
        window.maxSize = window.minSize;
        window.Show();

    }

    private void OnEnable()
    {
        this.splashTextures = Resources.LoadAll<Texture>("Bayat/PGAU/Editor/Splash");
        this.logoTexture = Resources.Load<Texture>("Bayat/PGAU/Editor/BayatLogo");
        List<Page> pagesList = new List<Page>();
        for (int i = 0; i < this.splashTextures.Length; i++)
        {
            pagesList.Add(new SplashPage(this.splashTextures[i]));
        }
        pagesList.Add(new ContactPage(this.logoTexture));
        this.pages = pagesList.ToArray();
    }

    private void OnDisable()
    {
        if (ShowOnStartup)
        {
            ShowOnStartup = false;
        }
        EditorPrefs.SetBool("Bayat.PGAU.WelcomeWindow.ShowOnStartUp", ShowOnStartup);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(960), GUILayout.Height(640));

        Page page = this.pages[this.currentPageIndex];
        page.OnPageGUI();

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        // Pagination
        EditorGUILayout.BeginHorizontal();
        Color defaultBGColor = GUI.backgroundColor;
        GUI.backgroundColor = Styles.PrimaryColor;
        if (GUILayout.Button("Back", Styles.RedButton))
        {
            this.currentPageIndex--;
            if (this.currentPageIndex < 0)
            {
                this.currentPageIndex = this.pages.Length - 1;
            }
        }
        if (GUILayout.Button("Next", Styles.RedButton))
        {
            this.currentPageIndex++;
            if (this.currentPageIndex >= this.pages.Length)
            {
                this.currentPageIndex = 0;
            }
        }
        GUI.backgroundColor = defaultBGColor;
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();
        GUILayout.Label("Made with ❤️ by Bayat", EditorStyles.centeredGreyMiniLabel);
    }

    public static class Styles
    {

        public static readonly Color PrimaryColor = new Color32(239, 83, 80, 255);

        static Styles()
        {
            CenteredLargeLabel = new GUIStyle(EditorStyles.largeLabel);
            CenteredLargeLabel.alignment = TextAnchor.MiddleCenter;
            CenteredLargeLabel.richText = true;

            RedButton = new GUIStyle(GUI.skin.button);
            RedButton.normal.textColor = Color.white;
        }

        public static readonly GUIStyle CenteredLargeLabel;
        public static readonly GUIStyle RedButton;

    }
}
