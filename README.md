# Human Resources Management System (HRMS)

## 📝 Overview

This project serves as a comprehensive, enterprise-grade Human Resources Management System (HRMS). Its core purpose is to provide a centralized, single source of truth for managing the entire lifecycle of an employee within an organization. Designed with a focus on robustness, modularity, and detailed data tracking, this application goes far beyond simple record-keeping. It acts as a sophisticated platform for managing professional growth, statutory requirements, and employee welfare.

## ✨ Key Features and Functionality

The strength of this system lies in its depth of coverage, handling a wide array of complex human resources needs:

### 👤 Employee Profile Management
*   **Comprehensive Records:** Stores complete personal data, including family members, passport information, and detailed military/registration records.
*   **Qualification Tracking:** Manages an extensive history of professional training and qualifications, allowing users to track the education and skill upgrades over time.
*   **Appointments & Roles:** Accurately records employment appointments and various roles within the organization.

### 🕒 Time & Leave Management
*   **Leave Tracking:** Provides dedicated views for tracking and managing vacation records, ensuring compliance and transparency.

### 🔎 Advanced Search & Reporting
*   **Intelligent Filtering:** The system includes sophisticated search capabilities, allowing users to filter and locate employees using multiple criteria defined by manageable search parameters.
*   **Statistical Analysis:** Dedicated service layers (e.g., `IStatisticsService`) calculate and present meaningful organizational statistics, enabling data-driven HR decisions.
*   **Data Export:** Seamless integration for exporting all viewed data types to external formats for further analysis.

### 🛡️ Statutory & Lifecycle Calculations
*   **Pension Age Calculation:** Features dedicated logic for managing and calculating complex entitlements, such as pension age settings, adhering to configurable organizational rules.
*   **Searchability:** The entire dataset is designed to be searchable by specific, nuanced parameters, ensuring that finding necessary records is fast and precise.

## 💡 Architectural Strengths

We prioritized clean code and scalability, resulting in a highly maintainable and extensible codebase.

*   **Model-View-ViewModel (MVVM) Pattern:** Adopting the MVVM pattern ensures a sharp separation between presentation logic (Views), business process handling (ViewModels), and pure data structure (Models). This makes the UI interchangeable and the business logic easily testable.
*   **Service Layer Focus:** Business logic is completely abstracted into dedicated services (e.g., `EmployeesService`, `IEmployeeSearchService`). This means core functionality (like searching or calculating) can be updated or replaced independently without touching the presentation layer.
*   **Dependency Injection Readiness:** The heavy use of interfaces (`IEmployeesService`, `IEmployeeSearchService`, etc.) makes the entire system highly testable and ready for sophisticated dependency injection frameworks, future-proofing the design.

## 🚀 Getting Started

1.  **Prerequisites:** Requires .NET Framework / .NET Core (Specify version if known).
2.  **Cloning:** Clone the repository: `git clone [repository-url]`
3.  **Build:** Restore dependencies and build the solution: `dotnet build`
4.  **Run:** Execute the application: `dotnet run`

---