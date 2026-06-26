\# WorkOrderFlow



A manufacturing workflow management application built with ASP.NET Core MVC.

The project focuses on managing customer-based work orders, connecting work order materials with inventory stock, and creating a clean foundation for real-world production/order tracking systems.



\## Overview



WorkOrderFlow is designed as a practical business application for small and medium-sized manufacturing operations.



The main goal is to model a realistic workflow where customer orders, work order materials, and stock movements can be managed in a structured web application.



This repository is part of my software development portfolio and demonstrates:



\* ASP.NET Core MVC application structure

\* Entity-based business workflow modeling

\* Customer and work order management logic

\* Work order material tracking

\* Inventory stock connection

\* Clean, maintainable project organization

\* Git-based development workflow



\## Features



\### Current Features



\* ASP.NET Core MVC web application

\* Customer-related workflow foundation

\* Work order material management

\* Inventory stock connection for work order materials

\* Structured Views, Controllers, and Models

\* Solution-based project organization

\* Clean Git history with incremental commits



\### Planned Improvements



\* Authentication and role-based authorization

\* Dashboard for active work orders

\* PDF export for work order documents

\* Database migrations with Entity Framework Core

\* Validation improvements

\* Unit and integration tests

\* Deployment-ready configuration



\## Tech Stack



\* C#

\* ASP.NET Core MVC

\* Entity Framework Core

\* HTML

\* CSS

\* JavaScript

\* Git / GitHub



\## Project Structure



```text

WorkOrderFlow/

├── WorkOrderFlow.Web/

│   ├── Controllers/

│   ├── Models/

│   ├── Views/

│   ├── wwwroot/

│   └── Program.cs

├── Views/WorkOrderMaterials/

├── WorkOrderFlow.sln

└── .gitignore

```



\## Getting Started



\### Prerequisites



Make sure the following tools are installed:



\* .NET SDK

\* Git

\* Visual Studio, Visual Studio Code, or JetBrains Rider



\### Clone the Repository



```bash

git clone https://github.com/OrcnTester/WorkOrderFlow.git

cd WorkOrderFlow

```



\### Restore Dependencies



```bash

dotnet restore

```



\### Build the Project



```bash

dotnet build

```



\### Run the Application



```bash

dotnet run --project WorkOrderFlow.Web

```



Then open the local URL shown in the terminal, usually:



```text

https://localhost:xxxx

```



or



```text

http://localhost:xxxx

```



\## Purpose of the Project



This project was created to simulate a real manufacturing workflow application.



The business scenario includes:



\* Receiving customer-related work orders

\* Managing production materials

\* Connecting required materials with inventory stock

\* Building a scalable foundation for future modules such as quotes, PDF documents, stock movements, and production planning



\## Portfolio Note



WorkOrderFlow is developed as a portfolio project to demonstrate backend and full-stack development skills with a real-world business use case.



The project reflects practical experience in building maintainable web applications for operational workflows.



\## Author



\*\*Orcun Yoruk\*\*

GitHub: \[OrcnTester](https://github.com/OrcnTester)



