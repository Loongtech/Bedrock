# LoongTech.Net 项目分析

这是一个基于 .NET 6 的后端基础库项目，名为 `LoongTech.Net`。它旨在为其他 .NET 项目提供一套通用的、可重用的核心功能，主要分为三个模块：**Core**、**Data** 和 **Logging**。

## 整体架构

项目采用模块化设计，每个模块都是一个独立的类库项目，职责清晰：

-   `Bedrock.Core`: 提供最基础的核心工具类，不依赖于任何特定框架。
-   `Bedrock.Data`: 提供一个统一的数据访问层，支持多种主流数据库。
-   `Bedrock.Logging`: 提供一套可定制的日志记录方案。

这种分层结构使得代码易于维护和扩展，每个模块都可以独立更新或被其他项目引用。

---

## 模块详解

### 1. Bedrock.Core (核心模块)

这是整个项目的基础，提供了一系列通用的辅助方法和扩展，以简化常见的编程任务。

#### 主要功能:

-   **集合扩展 (`CollectionExtensions.cs`)**:
    -   `ToDataTable<T>()`: 一个非常实用的扩展方法，可以将任何 `IEnumerable<T>` (如 `List<T>`) 高效地转换为 `System.Data.DataTable`。这在需要与一些旧的、依赖 DataTable 的 API (如批量插入) 交互时非常有用。
    -   **实现方式**: 内部通过反射获取类型 `T` 的所有公共属性，并以此为基础创建 DataTable 的列。然后遍历集合，将每个对象的属性值填充到 DataTable 的行中。

-   **字符串与日期扩展 (`StringExtensions.cs`)**:
    -   `ToUnixTimestamp()`: 将 `DateTime` 对象转换为 Unix 时间戳 (秒或毫秒)。
    -   `ToDateTimeFromUnix()`: 将 Unix 时间戳转换回 `DateTime` 对象。
    -   **实现方式**: 基于 `DateTime` 与 Unix 纪元 (`1970-01-01`) 之间的时间差 (`TimeSpan`) 进行计算。

-   **反射帮助类 (`ReflectionHelper.cs`)**:
    -   一个强大的静态帮助类，封装了 .NET 反射的复杂操作，如：
        -   动态加载程序集和类型 (`GetType`, `GetTypeFromAssemblyFile`)。
        -   动态创建对象实例 (`CreateInstance`)。
        -   动态获取和设置对象属性值 (`GetPropertyValue`, `SetPropertyValue`)。
        -   动态调用实例方法和静态方法 (`InvokeMethod`, `InvokeStaticMethod`)。
        -   扫描程序集以查找特定类型 (例如，所有实现了某个接口的类)。
    -   **实现方式**: 完全基于 `System.Reflection` 命名空间下的核心 API。

-   **类型帮助类 (`TypeHelper.cs`)**:
    -   `IsNullable()`: 判断一个类型是否为可空类型。
    -   `GetCoreType()`: 获取一个类型的核心基础类型 (例如，`int?` 的核心类型是 `int`)。
    -   **实现方式**: 利用 `Nullable.GetUnderlyingType()` 方法进行判断。

---

### 2. Bedrock.Data (数据访问模块)

这是项目的核心数据访问层，基于 **Dapper** 微型 ORM 构建，提供了对多种数据库的统一、高性能的访问接口。

#### 主要功能:

-   **统一的数据库操作接口**:
    -   `IDbHelper`: 定义了所有数据库帮助类都必须实现的标准 CRUD 操作，如 `QueryAsync`, `ExecuteAsync`, `ExecuteScalarAsync` 等。
    -   `IAdvancedDbHelper`: 定义了更高级的操作，如执行带输出参数的存储过程。

-   **多数据库支持**:
    -   `SqlServerHelper.cs`: 针对 SQL Server 的实现。
    -   `OracleHelper.cs`: 针对 Oracle 的实现。
    -   `MySqlHelper.cs`: 针对 MySQL 的实现。
    -   **实现方式**: 每个帮助类都实现了 `IDbHelper` 和 `IAdvancedDbHelper` 接口，内部使用 Dapper 配合相应数据库的 ADO.NET 驱动 (如 `Microsoft.Data.SqlClient`, `Oracle.ManagedDataAccess.Core`) 来执行操作。这种设计使得上层业务代码可以轻松地在不同数据库之间切换。

-   **高级功能**:
    -   **事务处理**: 提供了 `ExecuteTransactionAsync` 方法，可以原子性地执行多条 SQL 语句，确保数据一致性。
    -   **存储过程支持**: 完美支持存储过程的调用，包括处理输入/输出参数和返回结果集。
    -   **BLOB 处理**: 提供了 `OracleBlobParameter` 类来专门处理 Oracle 的 BLOB 数据类型，避免了常见的数据传输错误。
    -   **LIKE 查询转义**: `HandleLikeKey` 方法为不同数据库的 `LIKE` 查询提供了特殊字符的自动转义。

---

### 3. Bedrock.Logging (日志模块)

该模块提供了一套可插拔、可定制的日志记录解决方案，与 .NET 的标准日志框架 (`Microsoft.Extensions.Logging`) 深度集成。

#### 主要功能:

-   **日志帮助类 (`LoggerHelper.cs`)**:
    -   提供了一个静态的 `InitLoggerFactory` 方法来初始化整个日志系统，并提供了 `CreateLogger` 方法来创建日志记录器实例。
    -   **实现方式**: 内部使用 `Microsoft.Extensions.Logging.LoggerFactory` 作为核心，将不同的日志提供程序 (Provider) 添加到其中。

-   **自定义控制台日志 (`CustomConsoleFormatter.cs`)**:
    -   一个自定义的控制台日志格式化器，可以根据日志级别 (如 Info, Warning, Error) **以不同的颜色** 输出日志，极大地提高了日志的可读性。
    -   **实现方式**: 继承自 `ConsoleFormatter`，并重写 `Write` 方法，在输出前根据日志级别添加 ANSI 颜色代码。

-   **自定义文件日志 (`CustomFileLoggerProvider.cs`)**:
    -   一个自定义的文件日志提供程序，可以将日志写入到本地文件中。
    -   **功能特性**:
        -   日志文件会根据 **日志类别 (Category)** 和 **日期** 自动分目录、分文件存储。
        -   错误日志 (Error, Critical) 会被写入到单独的 `err-YYYY-MM-DD.txt` 文件中，便于快速定位问题。
    -   **实现方式**: 实现了 `ILoggerProvider` 和 `ILogger` 接口，内部使用线程锁 (`lock`) 来确保多线程写入文件的安全性。

## 总结

`LoongTech.Net` 是一个设计良好、功能实用的 .NET 基础库。它的模块化设计、对多种数据库的统一支持以及可定制的日志系统，都使其成为构建健壮、可维护的 .NET 应用程序的坚实基础。
