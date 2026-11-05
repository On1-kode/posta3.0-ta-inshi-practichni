# Компоненти програмної інженерії

Це окремий предмет. Роботи з ООП розташовані в папках ПР-37, ПР-42, ПР-46, ПР-47, ПР-51, ПР-53 і ПР-54 у корені репозиторію.

## Підсумковий застосунок

Відкрити [PostServiceSolution/PostService.slnx](PostServiceSolution/PostService.slnx) у Visual Studio з підтримкою Windows Forms. Для формату slnx потрібен актуальний SDK; global.json використовує встановлений SDK 10.0.401. Проєкти націлені на .NET 8.

Із папки PostServiceSolution:

```powershell
dotnet build PostService.slnx
dotnet run --project PostService/PostService.csproj
```

Сервер і вебклієнт: http://localhost:5082. Документація API: http://localhost:5082/swagger.

В іншому терміналі:

```powershell
dotnet run --project PostService.App/PostService.App.csproj
```

У Visual Studio є профіль `Сервер і Windows Forms` у PostService.slnLaunch для одночасного запуску обох проєктів. Під час першого використання слід вибрати цей профіль.

## Структура

- CommonTypes — DeliveryType.
- Models — ModelBase та Posting.
- Dtos — PostingPostDto, PostingPutDto, PostingGetDto з XML-коментарями.
- DataAccess — PostingDbContext, IGenericRepository<TModel>, GenericEfRepository<TModel>, IPostingRepository і PostingRepository.
- BusinessLogic — перевірка даних, розрахунок вартості та CRUD через репозиторій.
- PostService — Minimal API, мапер, Swagger та вебклієнт у wwwroot.
- PostService.App — список, редактор і HTTP-клієнт Windows Forms.

Залежності: API → BusinessLogic → DataAccess → Models → CommonTypes; API та App → Dtos → CommonTypes.

SQLite-файл postings-ef.db створюється біля збірки сервера. Дані зберігаються після перезапуску. Для іншого розташування можна задати змінну середовища POSTSERVICE_DB. Файли БД не додаються в Git.

Навчальні тарифи: відділення — 40 + 10 × вага; кур’єр — 70 + 15 × вага; експрес — 100 + 20 × вага. Клієнт не задає Price, Id і CreatedAt. Вага й розміри мають бути додатними, цінність — невід’ємною або порожньою.

## Відповідність практичним

| Практична | Де розташовано результат |
|---|---|
| 36 Minimal API | [Етапи/ПР-36 Minimal API](Етапи/ПР-36%20Minimal%20API) — GET / з точним текстом |
| 37 Простий REST API | [Етапи/ПР-37 REST API](Етапи/ПР-37%20REST%20API) — List<Posting> і початкові записи |
| 37 Узагальнені класи | [ПР-37 Узагальнені класи](ПР-37%20Узагальнені%20класи) — окремий консольний приклад статичної колекції |
| 38 REST Client | [requests.http](ПР-38%20REST%20Client/requests.http) — послідовний CRUD, запускати з етапом 37 на порту 5084 |
| 39 Бізнес-логіка | [Етапи/ПР-39 Бізнес логіка](Етапи/ПР-39%20Бізнес%20логіка) — DTO, мапер і сервіс у папках одного проєкту |
| 40 Компоненти | [Етапи/ПР-40 Компоненти](Етапи/ПР-40%20Компоненти) — сервер та чотири бібліотеки, .vscode для запуску й налагодження |
| 41 База даних | [Етапи/ПР-41 SQLite ADO.NET](Етапи/ПР-41%20SQLite%20ADO.NET) — параметризований SQL, CreateDb, postings-ado.db |
| 42 Windows Forms | [Етапи/ПР-42 Windows Forms](Етапи/ПР-42%20Windows%20Forms) — прямий HttpClient у формі, список і видалення |
| 43 EF Core | PostServiceSolution/PostService.DataAccess — контекст і generic repository |
| 44 Макети | [ПР-44 Макети](ПР-44%20Макети) — редагований diagrams.net та 16 PNG |
| 46 Обробка помилок | PostServiceSolution/PostService.App/PostServiceClient.cs, PostingList.cs, PostingEditor.cs, Program.cs |
| 47–48 Windows Forms | PostServiceSolution/PostService.App — список, спільний редактор, перевірка, збереження і скасування |
| 49–50 Web | PostServiceSolution/PostService/wwwroot — головна, про програму, список, деталі й редактор |
| 51 Swagger | PostServiceSolution/PostService/Program.cs, Swagger/IdParameterFilter.cs та XML-коментарі Dtos |
| 52 Інструкція | Окремий Word для Classroom; варіант 1 — Notepad, 9 mod 4 = 1 |
| 54 Планування | [ПР-54 Планування](ПР-54%20Планування) — Library Search Module.gan; PDF і звіт передаються окремо |

Проміжні етапи збережено, оскільки умови послідовно змінюють архітектуру: список у пам’яті → сервіс → бібліотеки → ADO.NET → EF Core. Кожний етап запускається окремо. Сервери етапів 39–42 використовують порт 5082 через launchSettings.json, тому їх не слід запускати разом із підсумковим сервером на тому самому порту.

Приклад запуску першого етапу з папки цього предмета:

```powershell
dotnet run --project "Етапи/ПР-36 Minimal API/PostService" --urls http://localhost:5083
```

Для REST Client етапу 38:

```powershell
dotnet run --project "Етапи/ПР-37 REST API/PostService" --urls http://localhost:5084
```

Для підсумкового API використовується PostServiceSolution/http-requests.rest. Запити виконуються зверху вниз; ідентифікатор береться з відповіді іменованого POST-запиту.

## Перевірка

Перевірено складання рішень та етапів, CRUD зі статусами 200/201/204/404, валідацію 400, автоматичну ціну, збереження даних SQLite після перезапуску та описи операцій, параметрів і полів у Swagger. Вебклієнт перевірено на вузькому екрані, при помилці мережі та видаленому записі. Зміну ваги у Windows-клієнті підтверджено через той самий запис у браузері.

Word-звіти не зберігаються в репозиторії.
