using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace Space_Invaders;

class Program
{
    static void Main(string[] args)
    {
        Game game = new Game();
        game.Run();
    }
}

//Class Game, который будет управлять нашей игрой.
public class Game
{    
    public const int WIDTH = 500;
    public const int HEIGHT = 800;
    private const string TITLE = "Space Invaders";
    private RenderWindow _window;
    private Sprite _background;
    private Player _player;
    private List<Bullet> _bullets;
    private bool _bulletDelay;
    private bool _shouldFire;
    private List<Enemy> _enemies;
    private int _maxEnemy = 3;
    public bool IsGameOver;

    //В конструкторе создадим переменную mode типа VideoMode, далее присвоим значение полю _window.
    //После добавим логику включения вертикальной синхронизации и вызов метода _window.Close() по событию.
    public Game()
    {
        var mode = new VideoMode(WIDTH, HEIGHT);
        _window = new RenderWindow(mode, TITLE);
                  
        _window.SetVerticalSyncEnabled(true);
        // Данная строка говорит нам о том, что в ответ на событие Closed
        // будет вызван фрагмент кода указанный после "=>", т.е. window.Close()
        _window.Closed += (_, _) => _window.Close();
        
        _background = new Sprite();
        _background.Texture = TextureManager.BackgroundTexture;
        _player = new Player();
        _bullets = new List<Bullet>();
        _enemies = new List<Enemy>();

    }
    public void Run()
    {
        //проверка, что окно не закрыто
        while (_window.IsOpen)
        {
            HandleEvents();
            Update();
            Draw();
        }
    }
    
    //Метод HandleEvents() будет отвечать за обработку событий в окне приложения,
    //таких как нажатия клавиш, перемещения мыши, клики и т.д.
    private void HandleEvents()
    {
        _window.DispatchEvents();
        _shouldFire = Keyboard.IsKeyPressed(Keyboard.Key.Space);
    }
    
    //Метод Update() будет обновлять игровую логику,
    //которая может включать в себя изменение положения объектов на экране и другие действия,
    //не связанные с графикой.
    private void Update()
    {
        int countVisibleEnemy = 0;
        _player.Update();

        foreach (Bullet bullet in _bullets)
        {
            bullet.Update();
        }

        foreach (Enemy enemy in _enemies)
        {
            enemy.Update();
        }
        
        // Выстрел
        if (!IsGameOver && _shouldFire)
        {
            if (!_bulletDelay)
            {
                _bullets.Add(new Bullet(new Vector2f(_player.Position.X + _player.Size.X/2,
                    _player.Position.Y), Bullet.SPEED_MIN));
                _bulletDelay = true;
            }
        }
        else
        {
            _bulletDelay = false;
        }
        
        // Удаление пуль вылетевших за пределы экрана или которые взорвались
        var bulletsCount = _bullets.Count;
        for (int i = 0; i < bulletsCount; i++)
        {
            if (_bullets[i].IsNeedDispose())
            {
                _bullets.Remove(_bullets[i]);
                bulletsCount = _bullets.Count;
                if (i != 0) i--;
            }
        }

        // создание врагов
        countVisibleEnemy = _enemies.Count;
        if (!IsGameOver && countVisibleEnemy < _maxEnemy)
        {
            _enemies.Add(new Enemy());
        }
        
        // Удаление вражин улетевших за экран и взорвавшихся
        var enemiesCount = _enemies.Count;
        for (int i = 0; i < enemiesCount; i++)
        {
            if (_enemies[i].IsNeedDispose())
            {
                _enemies.Remove(_enemies[i]);
                enemiesCount = _enemies.Count;
                if (i != 0) i--;
            }
        }

        // Поиск пуль попавших во вражин
        foreach (Bullet bul in _bullets)
        {
            foreach (Enemy enemy in _enemies)
            {
                if (bul.IsShottedEnemy(enemy))
                {
                    enemy.Explose();
                }
            }
        }

        foreach (Enemy enemy in _enemies)
        {
            if (!enemy.IsNeedExplose() && !_player.IsNeedExplose())
            {
                if (_player.Position.Y >= enemy.Position.Y &&
                    _player.Position.Y <= enemy.Position.Y + enemy.Size.Y)
                {
                    if (_player.Position.X + _player.Size.X >= enemy.Position.X && 
                        _player.Position.X <= enemy.Position.X + enemy.Size.X)
                    {
                        _player.Explose();
                        enemy.Explose();
                        IsGameOver = true;
                    }
                }
            }
        }
    }
    
    //Метод Draw() будет рисовать изображение на экране.
    private void Draw()
    {
        _window.Draw(_background);
        _player.Draw(_window);

        foreach (Enemy enemy in _enemies)
            enemy.Draw(_window);

        foreach (Bullet bul in _bullets)
           bul.Draw(_window);

        _window.Display();
    }
}

public class Player
{
    private const float PLAYER_SPEED = 4f;   
    private Sprite _sprite;
    public Vector2f Position;
    public Vector2f Size;
    private bool _needExplose;
    private int _exploseIndex;

    public Player()
    {
        _sprite = new Sprite();
        _sprite.Texture = TextureManager.PlayerTexture;
        
        //Vector2f - это структура данных в библиотеке SFML для представления двумерных векторов.
        //Она обычно используется для хранения координат точек или направлений в двумерном пространстве.
        //Для получения размеров спрайта, мы используем свойство TextureRect класса Sprite,
        //которое возвращает область текстуры спрайта. Затем мы используем свойства Width и Height текстуры,
        //чтобы получить ширину и высоту спрайта.
        Size = new Vector2f(_sprite.TextureRect.Width, _sprite.TextureRect.Height);
        
        //вычесление центра окна, и стартовая позиция игрока
        var screenCenter = new Vector2f(Game.WIDTH / 2f, Game.HEIGHT / 2f);
        var startPosition = screenCenter - Size / 2f;
        _sprite.Position = startPosition;
        Position = startPosition;
    }
    private void Move()
    {
        bool shouldMoveLeft = Keyboard.IsKeyPressed(Keyboard.Key.A);
        bool shouldMoveRight = Keyboard.IsKeyPressed(Keyboard.Key.D);
        bool shouldMoveUp = Keyboard.IsKeyPressed(Keyboard.Key.W);
        bool shouldMoveDown = Keyboard.IsKeyPressed(Keyboard.Key.S);
        bool shouldMove = shouldMoveLeft || shouldMoveRight || shouldMoveUp || shouldMoveDown;


        if (!shouldMove)
        {
            return;
        }

        if (shouldMoveLeft)
        {
            if (Position.X - PLAYER_SPEED >=0)
            {
                Position.X -= PLAYER_SPEED;
            }
        }

        if (shouldMoveRight)
        {
            if (Position.X + PLAYER_SPEED <= Game.WIDTH - _sprite.Texture.Size.X)
            {
                Position.X += PLAYER_SPEED;
            }
        }

        if (shouldMoveUp)
        {
            if (Position.Y - PLAYER_SPEED >= 0)
            {
                Position.Y -= PLAYER_SPEED;
            }
        }

        if (shouldMoveDown)
        {
            if (Position.Y + PLAYER_SPEED <= Game.HEIGHT - _sprite.Texture.Size.Y)
            {
                Position.Y += PLAYER_SPEED;
            }
        }
        
        _sprite.Position = Position;
    }
    
    public void Update()
    {
        Move();
    }

    public void Explose()
    {
        _needExplose = true;
        _exploseIndex = 0;
    }

    public bool IsNeedExplose()
    {
        return _needExplose;
    }

    public void Draw(RenderWindow window)
    {
        if (_needExplose)
        {
            if (_exploseIndex < 14)
            {
                if (_exploseIndex == 0)
                    _sprite.Texture = TextureManager.ExploseTexture;
                _sprite.TextureRect = new IntRect(new Vector2i((_exploseIndex % 4)*100, (_exploseIndex / 4)*100), new Vector2i(100, 100));
                _exploseIndex++;
            }
            else
            {
                _needExplose = false;
                _sprite = new Sprite();
            }
        }
        window.Draw(_sprite);
    }
}

public class Enemy
{
    private const float ENEMY_SPEED = 2f; 
    public Vector2f Position;
    private bool _needExplose;
    private bool _needDispose;
    private Sprite _sprite;
    private int _exploseIndex;
    public Vector2f Size;
    


    public Enemy()
    {
        _sprite = new Sprite();
        _sprite.Texture = TextureManager.EnemyTexture;
        var startPosition = new Vector2f(SpotEnemy(), -_sprite.Texture.Size.Y);
        _sprite.Position = startPosition;
        Position = startPosition;
        Size = new Vector2f(_sprite.Texture.Size.X, _sprite.Texture.Size.Y);
    }

    private float SpotEnemy()
    {
        Random coordX = new Random();

        return coordX.Next(0, (int) (Game.WIDTH - _sprite.Texture.Size.X));
    }

    private void Move()
    {
        if (!_needDispose && !_needExplose)
        {
            Position.Y += ENEMY_SPEED;
            
           if (_sprite.Position.Y > Game.HEIGHT)
           {
               _needDispose = true; 
           }
        }
        
        _sprite.Position = Position;
    }
    public void Update()
    {
        Move();
    }
    
    public void Draw(RenderWindow window)
    {
        if (_needExplose)
        {
            if (_exploseIndex < 14)
            {
                if (_exploseIndex == 0)
                    _sprite.Texture = TextureManager.ExploseTexture;
                _sprite.TextureRect = new IntRect(new Vector2i((_exploseIndex % 4)*100, (_exploseIndex / 4)*100), new Vector2i(100, 100));
                _exploseIndex++;
            }
            else
            {
                _needDispose = true;
                _needExplose = false;
            }
        }
        window.Draw(_sprite);
    }

    public void Explose()
    {
        _needExplose = true;
        _exploseIndex = 0;
    }

    public bool IsNeedDispose()
    {
        return _needDispose;
    }

    public bool IsNeedExplose()
    {
        return _needExplose;
    }
}

public class Bullet
{
    private Sprite _sprite;
    private float _speed;
    private bool _needDispose;
    private bool _needExplose;
    private int _exploseIndex = 0;
    public const float SPEED_MIN = 8f;
    
    public Bullet(Vector2f position, float speed)
    {
        _speed = speed;
        _sprite = new Sprite();
        _sprite.Texture = TextureManager.BulletTexture;
        _sprite.Scale = new Vector2f(0.3f, 0.3f);
        _sprite.Position = new Vector2f(position.X - (_sprite.Texture.Size.X * _sprite.Scale.X)/2,
            position.Y - (_sprite.Texture.Size.Y * _sprite.Scale.Y));
    }
    
    private void Move()
    {
        if (!_needDispose && !_needExplose)
        {
            _sprite.Position = new Vector2f(_sprite.Position.X, _sprite.Position.Y - _speed);
            if (_sprite.Position.Y < -(_sprite.Texture.Size.Y * _sprite.Scale.Y))
            {
                _needDispose = true;
            }
        }
    }

    public void Update()
    {
        Move();
    }
    
    public void Draw(RenderWindow window)
    {
        if (_needExplose)
        {
            if (_exploseIndex < 14)
            {
                if (_exploseIndex == 0)
                    _sprite.Texture = TextureManager.ExploseTexture;
                _sprite.TextureRect = new IntRect(new Vector2i((_exploseIndex % 4)*100, (_exploseIndex / 4)*100), new Vector2i(100, 100));
                _exploseIndex++;
            }
            else
            {
                _needDispose = true;
                _needExplose = false;
            }
        }
        window.Draw(_sprite);
    }
    
    public bool IsNeedDispose()
    {
        return _needDispose;
    }

    //метод проверяет попадение пули во врага
    public bool IsShottedEnemy(Enemy enemy)
    {
        bool result = false;

        // Если пуля не в статусе взрыва, она обрабатывается
        // с координатами врага для обнаружения попадания
        if (!enemy.IsNeedExplose() && !_needExplose && _sprite.Position.Y <= (enemy.Position.Y + enemy.Size.Y))
        {
            if (_sprite.Position.X + _sprite.Texture.Size.X * _sprite.Scale.X > enemy.Position.X && 
                _sprite.Position.X < enemy.Position.X + enemy.Size.X)
            {
                _needExplose = true;
                _exploseIndex = 0;
                result = true;
            }
        }

        return result;
    }
}

public class TextureManager
{    
    //При использовании абсолютных путей проект будет зависеть от конкретного расположения
    //файлов на вашем компьютере. Если перенесете его на другую машину,
    //то необходимо поменять абсолютный путь на свой.
    private const string ASSETS_PATH = "/Users/dark/RiderProjects/Space Invader/Space Invader/Assets";
    private const string BACKGROUND_TEXTURE_PATH = "/Backgrounds/purpleBG.png";
    private const string PLAYER_TEXTURE_PATH = "/Ships/playerShip1_orange.png";
    private const string BULLET_TEXTURE_PATH = "/Ships/ufoRed.png";
    private const string EXPLOSE_TEXTURE_PATH = "/Explosions/explosionsAtlas.png";
    private const string ENEMY_TEXTURE_PATH = "/Enemies/enemyBlack2.png";
    
    public static Texture BackgroundTexture;
    public static Texture PlayerTexture;
    public static Texture BulletTexture;
    public static Texture ExploseTexture;
    public static Texture EnemyTexture;
    
    //Статический конструктор в C# вызывается автоматически в момент первого обращения
    // к статическим членам класса, или перед созданием первого экземпляра класса.
    static TextureManager()
    {
        //новый объект класса Texture, в который передадим путь к текстуре,
        //используя конкатенацию строк, соединим ASSETS_PATH и BACKGROUND_TEXTURE_PATH.
        BackgroundTexture = new Texture(ASSETS_PATH + BACKGROUND_TEXTURE_PATH);
        
        //текстуры игрока. 
        PlayerTexture = new Texture(ASSETS_PATH + PLAYER_TEXTURE_PATH);
        
        //Текстура снаряда
        BulletTexture = new Texture(ASSETS_PATH + BULLET_TEXTURE_PATH);
        
        //Текстура взорванного снаряда
        ExploseTexture = new Texture(ASSETS_PATH + EXPLOSE_TEXTURE_PATH);
        
        //текстуры врага. 
        EnemyTexture = new Texture(ASSETS_PATH + ENEMY_TEXTURE_PATH);
    }
}
