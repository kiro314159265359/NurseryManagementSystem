# MonsterASP.NET deployment

This repository is configured for MonsterASP.NET's free Windows hosting plan,
.NET 10, and its included SQL Server database.

## 1. Create the free resources

In the MonsterASP control panel:

1. Create one free website.
2. Create one free Microsoft SQL Server database.
3. Enable HTTPS for the website.
4. Enable WebDeploy under **Deploy (FTP/WebDeploy/Git)**.

## 2. Configure application settings

Add these environment variables to the website. Never commit their values:

| Variable | Value |
| --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `DatabaseProvider` | `SqlServer` |
| `ConnectionStrings__DefaultConnection` | The SQL Server connection string from the database panel |
| `Jwt__Issuer` | `NurseryManagementSystem` |
| `Jwt__Audience` | `NurseryManagementSystem` |
| `Jwt__SecretKey` | A random secret of at least 32 characters |
| `AdminSeed__UserName` | The initial administrator username |
| `AdminSeed__Password` | A unique strong initial administrator password |

The application creates the empty SQL Server schema on first startup. After the
administrator signs in and changes the initial password, the two `AdminSeed__*`
variables can be removed.

## 3. Configure GitHub Actions

Add the following repository secrets under **Settings → Secrets and variables →
Actions** using the WebDeploy details shown by MonsterASP:

| GitHub secret | MonsterASP value |
| --- | --- |
| `WEBSITE_NAME` | Website name, such as `site12345` |
| `SERVER_COMPUTER_NAME` | WebDeploy URL, such as `https://site12345.siteasp.net:8172` |
| `SERVER_USERNAME` | WebDeploy username |
| `SERVER_PASSWORD` | WebDeploy password |

Every push to `master` then builds and deploys the API. The workflow can also be
started manually from the repository's **Actions** page.

## Free-plan constraints

The free plan is appropriate for demonstrations and learning. It provides one
website, 256 MB RAM, one 1 GB database, a MonsterASP subdomain, limited traffic,
and no availability guarantee.
