using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct TemplateInfo
{
    public string name;
    public int count_vertices;
    public List<Vector3> vertices;

    public TemplateInfo(string name, int count_vert, List<Vector3> vert)
    {
        this.name = name;
        count_vertices = count_vert;
        vertices = new List<Vector3>();
        vertices.InsertRange(0, vert);
    }
}

class Shape
{
    protected GameObject instance;
    protected LineRenderer shape;
    protected List<Vector3> points;
    protected bool[,] arr;
    float line_width;
    public const int size = 16;

    public Shape(GameObject inst, float line_w)
    { 
        instance = inst;
        line_width = line_w;
        CreateNewLineRenderer();
    }

    public bool IsContainPoint(Vector3 coords)
    {
        return points.Contains(coords);
    }

    public List<Vector3> GetPoints()
    {
        return points;
    }

    public LineRenderer GetShape()
    {
        return shape;
    }

    public bool[,] GetArr()
    {
        return arr;
    }

    void CreateNewLineRenderer()
    {
        shape = instance.AddComponent<LineRenderer>();
        shape.material = new Material(Shader.Find("Diffuse"));
        shape.SetVertexCount(0);
        shape.SetWidth(line_width, line_width);
        shape.SetColors(Color.black, Color.black);
        shape.useWorldSpace = true;
        points = new List<Vector3>();
    }

    public void AddVertex(Vector3 coords)
    {
        points.Add(coords);
        shape.SetVertexCount(points.Count);
        shape.SetPosition(points.Count - 1, (Vector3) points[points.Count - 1]);
    }

    float Angle(Vector3 start, Vector3 finish)
    {
        return Mathf.Atan2(finish.y - start.y, finish.x - start.x);
    }


    public void FillArr(bool is_target)
    {
        float minx = points[0].x;
        float maxx = points[0].x;
        float miny = points[0].y;
        float maxy = points[0].y;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].x > maxx)
                maxx = points[i].x;
            if (points[i].x < minx)
                minx = points[i].x;
            if (points[i].y > maxy)
                maxy = points[i].y;
            if (points[i].y < miny)
                miny = points[i].y;
        }
        //additional points for target shape
        if (is_target)
        {
            int tp = points.Count;
            for (int i = 0; i < tp; i++)
            {
                Vector3 add = points[i];
                float a = Angle(points[i], points[i + 1]);
                float x = Mathf.Cos(a) * 10;
                float y = Mathf.Sin(a) * 10;
                float mx;
                float my;
                do
                {
                    add = new Vector3(x + add.x, y + add.y, 0);
                    points.Add(add);
                    mx = (add.x - points[i + 1].x);
                    my = (add.y - points[i + 1].y);
                } while (Mathf.Sqrt(mx * mx + my * my) > 20f);
            }
        }
        arr = new bool[size, size];
        float diffx = Mathf.Abs(maxx - minx);
        float diffy = Mathf.Abs(maxy - miny);
        for (int i = 0; i < points.Count; i++)
        {
            int x = (int)(((points[i].x - minx) / diffx) * (size - 1));
            int y = (int)(((points[i].y - miny) / diffy) * (size - 1));
            int sizey = (size - 1) - y;
            int sizex = (x > 0) ? x : 0;
            try
            {
                arr[sizey < 1 ? 0 : sizey, sizex] = true;
            }
            catch (System.Exception) { Debug.Log("sizey = " + sizey + " sizex = " + sizex); }
        }
    }

    public virtual void RemoveShape()
    {
        shape.SetVertexCount(0);
        points.RemoveRange(0, points.Count);
    }
}

class Figure : Shape
{
    List<TemplateInfo> figures_list;
    public Figure(GameObject inst, float line_w, List<TemplateInfo> FIGURES_LIST)
        : base(inst, line_w)
    {
        figures_list = FIGURES_LIST;
        CreateNewShape();
    }

    public void CreateNewShape()
    {
        int figure = Random.Range(0, figures_list.Count);
        int rnd = 50;
        for (int i = 0; i < figures_list[figure].count_vertices; i++)
            AddVertex(instance.transform.position + figures_list[figure].vertices[i] + 
                new Vector3(Random.Range(-rnd, rnd), Random.Range(-rnd, rnd), 0));
        AddVertex(points[0]);
    }

    public override void RemoveShape()
    {
        base.RemoveShape();
        CreateNewShape();
    }
}

public class game : MonoBehaviour  
{    
	GameObject particles;
    Figure target;
    Shape user;
	bool isMousePressed;   
	bool wasted;
	bool HELP;
	float time_start = 20;
	float time_left;
	int score;

	void Start()    
	{
        int scale = 100;
        List<TemplateInfo> FIGURES_LIST = new List<TemplateInfo>();
        List<Vector3> vectors = new List<Vector3>();
        vectors.Add(new Vector3(-1, -1, 0) * scale);
        vectors.Add(new Vector3(0, 1, 0) * scale);
        vectors.Add(new Vector3(1, 1, 0) * scale);
        FIGURES_LIST.Add(new TemplateInfo("triangle", 3, vectors));
        vectors.RemoveRange(0, vectors.Count);

        vectors.Add(new Vector3(-1, -1, 0) * scale);
        vectors.Add(new Vector3(-1, 1, 0) * scale);
        vectors.Add(new Vector3(1, 1, 0) * scale);
        vectors.Add(new Vector3(1, -1, 0) * scale);
        FIGURES_LIST.Add(new TemplateInfo("rectangle", 4, vectors));
        vectors.RemoveRange(0, vectors.Count);


        target = new Figure(GameObject.FindGameObjectWithTag("Target"), 10f, FIGURES_LIST);
        user = new Shape(gameObject, 2f);
        particles = GameObject.FindGameObjectWithTag("Particles");
		time_left = time_start;
		score = 0;
		wasted = false;
		HELP = false;
	}

	bool ShapeMatch ()
	{
		user.FillArr(false);
		target.FillArr(true);
		int error = 0;
		int win = 0;
		int border = 5;
		for (int i = 0; i < Shape.size; i++)
            for (int j = 0; j < Shape.size; j++)
			{
				if(user.GetArr()[i, j] && target.GetArr()[i, j])
					win++;
                if (user.GetArr()[i, j] != target.GetArr()[i, j]) error++;
			}
        Debug.Log("%ERROR = " + error + " win " + win + " point " + (Shape.size * Shape.size) / 3);
        if (win == 0 || error > (Shape.size * Shape.size) / 3 || error / win >= border)
			return false;
		else
			return true;
	}

	void LevelResult ()
	{
		if (user.GetPoints().Count > 1 && ShapeMatch ())
		{
			score++;
			if (time_start > 3)
				time_start -= 0.25f;
			time_left = time_start;
            target.RemoveShape();
		} 
	}

   void OnGUI()
	{
		GUI.contentColor = Color.black;
        if (HELP)
        {
            bool[,] user_arr = user.GetArr();
            bool[,] target_arr = target.GetArr();
            for (int i = 0; i < Shape.size; i++)
                for (int j = 0; j < Shape.size; j++)
                {
                    GUI.Label(new Rect((j + 1) * 10, (i + 2) * 10, 20, 20), user_arr[i, j] ? "+" : "-");
                    GUI.Label(new Rect(200 + (j + 1) * 10, (i + 1) * 10, 20, 20), target_arr[i, j] ? "+" : "-");
                }
        }
		if (!wasted)
			GUI.Label (new Rect (Screen.width / 2 - 50, 10, 150, 20), "Time left: " + time_left);
		else
		{
			GUI.Label (new Rect (Screen.width / 2 - 50, Screen.height / 2 - 200, 150, 20), "Your score = " + score); 
			if(GUI.Button (new Rect (Screen.width / 2 - 100, Screen.height / 2 - 100, 200, 50), "Restart"))
				Application.LoadLevel(1);
		}
	}

	void Update ()      
	{        
		if(!wasted)
		{
			if (time_left > 0)
				time_left -= Time.deltaTime;
			else
				wasted = true; 
			if(Input.GetMouseButtonDown(0))  
			{
				isMousePressed = true; 
				particles.particleSystem.Play();
			}
			else if(Input.GetMouseButtonUp(0))        
			{   
				particles.particleSystem.Stop();
				if (user.GetPoints().Count > 3)
					LevelResult ();
                user.RemoveShape();
				isMousePressed = false; 
			}        
			if(isMousePressed)        
			{            
				Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
				particles.transform.position = mousePos;
				mousePos.z = -1;

                if (!user.IsContainPoint(mousePos))              
                    user.AddVertex(mousePos);
			}
		}
		if (Input.GetKeyDown(KeyCode.Space))
			if (HELP) HELP = false;
		else HELP = true;
		if (Input.GetKeyDown (KeyCode.Escape))
			Application.Quit ();
	}
}