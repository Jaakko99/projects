using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
/// <summary>
/// @author Jaakko Peurasaari
/// "version 26.11.2020
/// todo: Lisää kenttiä
/// Liikkumisen korjaaminen
/// Räjähdyksen tapahtuminen hahmon koordinaatilla
/// Pelin lopettaminen kun on tuhonnut tarpeeksi ufoja
/// </summary>

class Ufo : PhysicsObject
{
 
    private IntMeter healthbar = new IntMeter(3, 0, 3);
    public IntMeter Healthbar { get { return healthbar; } }

    public Ufo(double leveys, double korkeus)
        : base(leveys, korkeus)
    {
        healthbar.LowerLimit += delegate { this.Destroy(); };
        
    }
}


public class avaruuspeli : PhysicsGame
{

    private LaserGun p1laser;
    private Image pic = LoadImage("alus");
    private Image pic2 = LoadImage("ufo");
    private Image pic3 = LoadImage("laser");
    private Image pic4 = LoadImage("vihu2");
    private Image pic5 = LoadImage("tausta1");
    private Image pic6 = LoadImage("healthbar1");
    private IntMeter pointcounter;
    private IntMeter starcounter;
    private readonly int tahtia = 50;
    DoubleMeter lifeCount;
    
    

    /// <summary>
    /// Pelin aloittaminen
    /// </summary>
    public override void Begin()
    {       
        Menu();            
    }
    
    /// <summary>
    /// Aliohjelma vihun synnyttämiselle
    /// </summary>
   private void AddEnemies()
    {

        Ufo enemy = new Ufo(100,100); 
        Add(enemy);
        enemy.LifetimeLeft = TimeSpan.FromSeconds(5.0);
        enemy.Image = LoadImage("ufo");
        enemy.Tag = "enemy";
        AddCollisionHandler(enemy, "player", EnemyCollide);       
        /// Luodaan viholliselle aivot, jonka avulla viholliset voivat jahdata pelaajaa  
        RandomMoverBrain enemybrain = new RandomMoverBrain(500);  ///satunnaisaivot liikkuvat nopeudella 200
        enemy.Brain = enemybrain;
        enemybrain.ChangeMovementSeconds = 2;
        enemy.Healthbar.Value--;
        FollowerBrain followerBrain = new FollowerBrain("player");
        followerBrain.Speed = 300;
        followerBrain.DistanceFar = 400;
        followerBrain.DistanceClose = 200;
        followerBrain.FarBrain = enemybrain;
       
    }
    /// <summary>
    /// Luodaan powerup pelaajalle
    /// </summary>
  
    


    ///<summary>
    ///Luodaan valikko
    ///<summary>

    private void Menu()
    {
        ClearAll();   // tyhjennetään kenttä kaikista pelaajista
        List<Label> menusection = new List<Label>();
        Label section1 = new Label("Aloita uusi peli");  //// Luodaan uusi Label-olio, joka toimii uuden pelin aloituskohtana
        section1.Position = new Vector(0, 40); // Asetetaan valikon ensimmäinen kohta keksikohdan yläpuolelle
        menusection.Add(section1);
        Label section2 = new Label("Lopeta peli");
        section2.Position = new Vector(0, 0);
        menusection.Add(section2);
        Mouse.ListenOn(section1, MouseButton.Left, ButtonState.Pressed, StartGame, null);

        //Lisätään kaikki luodut kohdat peliin foreach-silmukalla
        foreach (Label menuSection in menusection)
        {
           
            Add(menuSection);
        }          

    }

    void CreateHealthBar()
    {
        /// aliohjelma kuolemiselle
        lifeCount = new DoubleMeter(10);
        lifeCount.MaxValue = 10;
        ProgressBar healthbar = new ProgressBar(150, 20);
        healthbar.X = Screen.Right + 150;
        healthbar.Y = Screen.Top - 20;
        healthbar.BindTo(lifeCount);
        Add(healthbar);
        healthbar.Color = Color.Transparent;
        healthbar.BarImage = pic6;
        healthbar.BorderColor = Color.Black;
        
    }


    /// <summary>
    /// Luodaan satunaisia tähtiä
    /// </summary>
    /// param name = "game" peli johon kolmio luodaan 
    /// <param name = "speed" pelaaja joka ohjataan  </param>   
    /// <param name = "tag" tähdelle annettaba tunniste </param>
    private static PhysicsObject RandomStar(PhysicsGame game, BoundingRectangle rect, double speed, string tag)
    {
        double width = RandomGen.NextDouble(5, 20);
        double height = RandomGen.NextDouble(5, 20);
        PhysicsObject star = new PhysicsObject(width, height, Shape.Star);
        star.Position = RandomGen.NextVector(rect);
        star.Angle = RandomGen.NextAngle();
        star.Color = Color.Yellow;
        Vector direction = RandomGen.NextVector(0, speed);
        star.Hit(direction);
        star.Tag = tag;
        game.Add(star);
        return star;      
    }

 
    /// Pelaajan liikuttaminen
    /// </param name = "p1" lyötävä pelaaja>
    private static void MovePlayer(PhysicsObject p1, Vector direction)
    {
        p1.Hit(direction);
    }


    /// <summary>
    /// Aliohjelma vihujen törmäys pelaajaan
    /// </summary>
    /// <param name="enemy"></param>
    /// <param name="player"></param>
    private void EnemyCollide(PhysicsObject enemy, PhysicsObject player)
    {
        Explosion explosion = new Explosion(500);
        Add(explosion);
        player.Destroy();
        Timer.SingleShot(8, StartGame);
        
    }


    /// <summary>
    /// Aliohjelma ammuksen osumiselle
    /// </summary>
    /// <param name="p1laser"></param>
    /// <param name="target"></param>
    private void BulletHit(PhysicsObject p1laser, PhysicsObject target)
    {
        if (target.Tag.ToString().Equals("enemy"))
        {
            Explosion explosion = new Explosion(80);   
            Add(explosion);
            target.Destroy();
            pointcounter.Value += 1;                       
        }
              
    }


    /// <summary>
    /// Aliohjelma aseen ampumiselle
    /// </summary>
    /// <param name="gun"></param>
    private void ShootGun(LaserGun gun)
    {
        PhysicsObject bullet = gun.Shoot();
        if (bullet != null)
        {
            bullet.Size *= 5;
            bullet.Image = LoadImage("laser");
            gun.CanHitOwner = false;
            gun.InfiniteAmmo = true;
            gun.FireRate = 40.0;
            gun.AmmoIgnoresExplosions = true;
            bullet.MaximumLifetime = TimeSpan.FromSeconds(2.0);
        }
    }


    /// <summary>
    /// Aliohjelma tähtien keräykseen
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="star"></param>
    private void StarCollect(PhysicsObject p1, PhysicsObject star)
    {
        MessageDisplay.Add("Keräsit tähden");
        star.Destroy();
        starcounter.Value += 10;
    }
    ///<summary>
    /// Aliohjelma powerupin keräämiseen
    /// </summary>
    private void PowerUpCollected(PhysicsObject p1, PhysicsObject power)
    {
        MessageDisplay.Add("Power-up collected");
        p1.Destroy();

    }
    
    ///<summary>
    ///Aliohjelma powerupin keräämiseen
    ///</summary>
    
    
    /// <summary>
    /// Ohjelma tähtäämiseen
    /// </summary>
    private void Tahtaa()
    {
        Vector direction = (Mouse.PositionOnWorld - p1laser.AbsolutePosition).Normalize();
        p1laser.Angle = direction.Angle;
    }


    /// <summary>
    /// Luodaan pistelaskuri, joka
    /// laskee tuhotut viholliset
    /// </summary>
    private IntMeter CreatePointCounter(string counter)
    {
        IntMeter pointcounter;       
        pointcounter = new IntMeter(0);
        starcounter = new IntMeter(0);
        Label pointscreen = new Label();
        Label pointscreen2 = new Label();
        pointscreen.X = Screen.Left + 100;
        pointscreen.Y = Screen.Top - 100;      
        pointscreen.TextColor = Color.Aqua;
        pointscreen.Color = Color.White;
        pointscreen.BindTo(pointcounter);
        pointscreen2.X = Screen.Bottom + 700;
        pointscreen2.Y = Screen.Right + 1200;
        pointscreen2.TextColor = Color.Aqua;
        pointscreen2.Color = Color.White;
        pointscreen2.BindTo(starcounter);
        Add(pointscreen);
        Add(pointscreen);
        pointscreen.Title = counter;
        starcounter.MaxValue = 10;
        starcounter.UpperLimit += StarsCollected;
        return pointcounter;
       
        
    }
    /// <summary>
    /// Pelin aloittainen
    /// Aliohjelmassa kaikki asiat syntyvät
    /// </summary>

    private void StartGame()
    {       
        ClearAll();
        pointcounter = CreatePointCounter("Ufos destoryed");
        starcounter = CreatePointCounter("Stars collected");

        //MediaPlayer.Play("musiikkia.wav");   
        //IsFullScreen = true;
        Level.CreateBorders();
        Level.Background.Image = pic5;
        Pause();
        MessageDisplay.TextColor = Color.Aqua;
        MessageDisplay.Add("SpaceGame");      
        Mouse.IsCursorVisible = true;

        Timer.CreateAndStart(1.5, AddEnemies);       

        BoundingRectangle bottompart = new BoundingRectangle(new Vector(Level.Left, 0), Level.BoundingRect.BottomRight);
        BoundingRectangle top = new BoundingRectangle(Level.BoundingRect.TopLeft, new Vector(Level.Right, 0));

        for (int i = 0; i < tahtia; i++)
            RandomStar(this, bottompart, 50, "tahti");

        PhysicsObject player = new PhysicsObject(2 * 30, 2 * 30);
        Add(player);
        player.Tag = "player";
        player.Image = LoadImage("alus");
        p1laser = new LaserGun(30, 10);
        p1laser.ProjectileCollision = BulletHit;
        player.Add(p1laser);
        AddCollisionHandler(player, "tahti", StarCollect);

        ///<summary>
        ///Näppainasetukset
        ///</summary>
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "näytä avustus");
        Keyboard.Listen(Key.Up, ButtonState.Pressed, MovePlayer, "Pelaaja ylös", player, new Vector(0,100));
        Keyboard.Listen(Key.Down, ButtonState.Pressed, MovePlayer, "Pelaaja alas", player, new Vector(0, -100));
        Keyboard.Listen(Key.Left, ButtonState.Pressed, MovePlayer, "Pelaaja vasemalle", player, new Vector(-100, 0));
        Keyboard.Listen(Key.Right, ButtonState.Pressed, MovePlayer, "Pelaaja oikealle", player, new Vector(100, 0));
        Mouse.Listen(MouseButton.Left, ButtonState.Pressed, ShootGun, "Ammu", p1laser);
        Mouse.ListenMovement(0.1, Tahtaa, "Tähtää aseella");
        Keyboard.Listen(Key.P, ButtonState.Pressed, Pause, "Pysäyttää pelin");
        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

    }


    /// <summary>
    /// Tehdään aliohjelma pelin voittamiselle
    /// </summary>
    private void StarsCollected()
    {
        Keyboard.Listen(Key.R, ButtonState.Pressed, StartGame, "Aloita peli-alusta");
        Label lopetus = new Label(60.0, 20.0, "Voitit Pelin");
        Add(lopetus);      
    }

   

    /// <summary>
    /// Aliohjelma pelin lopettamiselle(kesken)
    /// </summary>
    private void GameOver()
    {
        StarsCollected();
        
    }




}

