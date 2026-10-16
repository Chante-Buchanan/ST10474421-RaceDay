#raceday
<img width="959" height="563" alt="SCREENSHOT  CI CD" src="https://github.com/user-attachments/assets/1754c335-5a7b-4a96-b257-7c1e69fcc163" />
CI — Continuous Integration Developers frequently merge their code changes into a shared repository (often several times a day). Each merge triggers an automated process that builds the code and runs tests, so problems get caught early instead of piling up until a big, painful merge at the end of a project.
CD — Continuous Delivery / Deployment
•	Continuous Delivery: every change that passes the automated tests is automatically prepared for release (packaged, ready to deploy), but a human still decides when to actually push it to production.
•	Continuous Deployment: goes one step further — every change that passes the tests is automatically deployed to production with no manual approval step.
https://youtu.be/IeXTrviJ8_g

PART2
https://youtu.be/XZPsFvt1QSg

Step 1: Project setup 
1.	In Visual Studio, create an ASP.NET Core Web API project (.NET 8 is a safe choice).
2.	Install NuGet packages: 
o	Microsoft.EntityFrameworkCore.SqlServer
o	Microsoft.EntityFrameworkCore.Tools
o	Swashbuckle.AspNetCore (Swagger)
3.	Put your connection string in appsettings.json.
4.	Use a clear folder structure: Models, DTOs, Data, Controllers, Services (optional).
Step 2: Models and database 
1.	Create entity classes exactly matching your ERD. They will likely include User, Event, Category, Enrolment, and Result.
2.	Create AppDbContext : DbContext with a DbSet for each entity. Use OnModelCreating for relationships, for example Enrolment linking User, Event, and Category.
3.	Run Add-Migration InitialCreate and then Update-Database.
4.	Compare the generated tables to your Part 1 script. If they differ, fix the models, because the brief says the schema must match.
Tip: Store event type (run/walk/cycle) as an enum. It is cleaner and easy to validate.
Step 3: Authentication and roles (the hardest part)
This is where many students lose marks, so take your time.
The brief says session management, not JWT. That is deliberate, and I'd follow it literally.
•	Register: accept personal details, password, and role. The note "(Think about how this should be managed)" is a hint. Anyone can pick "Organiser" at sign-up, which is a security question. At minimum, validate that the role is only Organiser or Participant. Better, mention in your documentation that in a real system Organiser accounts would need approval or an invite code. You could even implement a simple organiser registration code. Explain whichever choice you make.
•	Password hashing: never store plain text. Use PasswordHasher<User> from ASP.NET Core Identity (easiest), or BCrypt (BCrypt.Net-Next).
•	Login: find the user, verify the hash, then store UserId and Role in the session using HttpContext.Session.
•	Session setup: in Program.cs, call AddDistributedMemoryCache(), AddSession(), and app.UseSession().
•	Logout: clear the session.
•	Protecting endpoints: a custom authorization filter or attribute is the neatest approach, for example [SessionAuthorize] for "any logged-in user" and [SessionAuthorize(Role = "Organiser")] for organiser-only. It checks the session and returns 401 if not logged in or 403 if the wrong role. Reusing one attribute means less repeated code and cleaner tests.
Step 4: Build the controllers
Work through them one at a time, testing each in Swagger before moving on:
Controller	Key endpoints	Who
Auth	register, login, logout	Public
Users/Profile	GET and PUT own profile	Both (own data only)
Events	GET all / GET by id	Both
	POST, PUT, DELETE	Organiser
Categories	GET per event	Both
	POST, PUT, DELETE	Organiser
Enrolments	POST (enter an event with a category)	Participant
	GET all for my events	Organiser
Results	POST/PUT finish time and position	Organiser
	GET my results	Participant
Some practical points:
•	Use DTOs for requests and responses so you never return the password hash.
•	Take the user's ID from the session, not from the request body. Otherwise one user can edit another's profile.
•	An Organiser should only manage or view enrolments for their own events. Check event.OrganiserId == sessionUserId.
•	Block duplicate enrolments, and check that the category belongs to that event.
•	Return proper status codes: 200, 201, 204, 400, 401, 403, 404, 409.
Step 5: Swagger
•	Swagger is mostly set up by the template. Add XML comments (/// <summary>) to each action, then enable GenerateDocumentationFile in the .csproj and include the XML file in AddSwaggerGen.
•	Add [ProducesResponseType] attributes so the responses are documented.
•	The brief wants each endpoint to show its purpose, expected data, and response. XML comments plus ProducesResponseType cover that.
Step 6: Unit tests
1.	Add an xUnit test project and reference your API project.
2.	Use the EF Core InMemory provider (or SQLite in-memory) so tests don't need your real database.
3.	Mock the session, or use a small fake HttpContext with a session.
4.	Cover the required areas, each with a success case and a failure case: 
o	Registration and login: valid registration succeeds, duplicate email fails, wrong password returns 401, correct login sets the session.
o	Event management: an Organiser can create, update, and delete, and a Participant is rejected.
o	Role rejection: a Participant calling an organiser endpoint gets 403, and the reverse, and an unauthenticated user gets 401.
o	Enrolments: a Participant can enrol, the Participant, event, and category link is stored correctly, a duplicate is rejected, and an invalid category is rejected.
5.	Aim for roughly 15 to 25 well-named tests, such as CreateEvent_AsParticipant_ReturnsForbidden.
Step 7: GitHub Actions CI/CD
1.	Push the project to GitHub early so you have a commit history.
2.	Create .github/workflows/ci.yml:
name: CI
on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
jobs:
  build-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release
      - run: dotnet test --no-build --configuration Release
3.	Make sure a green run exists and screenshot it for your submission. Never commit your real connection string or secrets.
Suggested order and timing
1.	Setup, models, migration (get this right first)
2.	Auth, session, and the authorization attribute
3.	Events and Categories
4.	Enrolments and Results
5.	Profile endpoints
6.	Swagger comments
7.	Unit tests (write them as you go, don't leave them to the last night)
8.	GitHub Actions and final checks
Advice
•	Don't leave auth until last. Every other endpoint depends on it.
•	Test in Swagger as you build. Note that session cookies work in the browser, so Swagger UI will keep you logged in after calling login.
•	Match Part 1 exactly. If you must change something, such as a field name or an endpoint, note it and justify it.
•	Commit often with meaningful messages. Markers can see your progress.
•	Check the rubric for extra deliverables such as screenshots, a README, or a demo, and plan time for them.
•	Common mark-losers: plain-text passwords, no role checks on some endpoints, users accessing other users' data, tests that only cover the happy path, and a failing pipeline.
If you tell me where you're stuck, I can go deeper on any step, for example the session-based role attribute, the DbContext relationships, or the xUnit setup with a mocked session. I can also write sample code for them.
 

