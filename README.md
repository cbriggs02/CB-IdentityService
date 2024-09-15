# CB2P-Web-App
This project is a REST API (an interface that allows clients to communicate with a server using standard web protocols) built using the ASP.NET Web API framework (a set of tools and libraries for building web applications) to create HTTPS services (secure web services) that can be accessed from clients via a browser. The system is designed with a modular approach (dividing the application into smaller, independent parts), breaking the application into interfaces (blueprints for how components should behave), services (classes that handle business logic), controllers (which manage HTTP requests), and models (objects that represent the data structure).

Using ASP.NET models, I have encapsulated all data for different entities (objects that represent key parts of the system, like users), data transfer objects (simplified objects used to move data between layers), expected requests, and results. This system communicates with an MSSQL Server (a Microsoft database management system) leveraging Entity Framework Core (an open-source ORM or object-relational mapper that simplifies database operations) to streamline data access.

The API is documented using Swagger UI (a tool for visualizing and interacting with the API) and uses ASP.NET Identity (a library for managing user authentication) to authenticate users with JWT (JSON Web Tokens, a compact token format used for secure transmission of information), manage user data and passwords. This project is monitored using Azure DevOps (a set of tools for software development and project management) to ensure proper project management.
