# Json-Response-To-Sql-Query
A simple little helper app that allows you to take a Json response and create a Sql Server Select statement from it

Consider the following results in Json format from [Covid-19 API](https://covid19-api.org/)

```json
[
  {
    "country": "US",
    "last_update": "2020-04-24T06:31:06",
    "new_cases": 2526,
    "new_deaths": 204,
    "new_recovered": 996,
    "new_cases_percentage": 0.29,
    "new_deaths_percentage": 0.41,
    "new_recovered_percentage": 1.25
  },
  {
    "country": "MX",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 1089,
    "new_deaths": 99,
    "new_recovered": 0,
    "new_cases_percentage": 10.33,
    "new_deaths_percentage": 10.21,
    "new_recovered_percentage": 0
  },
  {
    "country": "BR",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 544,
    "new_deaths": 18,
    "new_recovered": 0,
    "new_cases_percentage": 1.1,
    "new_deaths_percentage": 0.54,
    "new_recovered_percentage": 0
  },
  {
    "country": "UA",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 477,
    "new_deaths": 6,
    "new_recovered": 97,
    "new_cases_percentage": 6.65,
    "new_deaths_percentage": 3.21,
    "new_recovered_percentage": 19.25
  },
  {
    "country": "IN",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 463,
    "new_deaths": 1,
    "new_recovered": 0,
    "new_cases_percentage": 2.01,
    "new_deaths_percentage": 0.14,
    "new_recovered_percentage": 0
  },
  {
    "country": "JP",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 418,
    "new_deaths": 29,
    "new_recovered": 70,
    "new_cases_percentage": 3.5,
    "new_deaths_percentage": 9.7,
    "new_recovered_percentage": 4.92
  },
  {
    "country": "PA",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 174,
    "new_deaths": 2,
    "new_recovered": 16,
    "new_cases_percentage": 3.49,
    "new_deaths_percentage": 1.39,
    "new_recovered_percentage": 6.27
  },
  {
    "country": "AR",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 147,
    "new_deaths": 6,
    "new_recovered": 0,
    "new_cases_percentage": 4.47,
    "new_deaths_percentage": 3.77,
    "new_recovered_percentage": 0
  },
  {
    "country": "HU",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 99,
    "new_deaths": 11,
    "new_recovered": 11,
    "new_cases_percentage": 4.33,
    "new_deaths_percentage": 4.6,
    "new_recovered_percentage": 2.82
  },
  {
    "country": "PK",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 98,
    "new_deaths": 2,
    "new_recovered": 190,
    "new_cases_percentage": 0.89,
    "new_deaths_percentage": 0.85,
    "new_recovered_percentage": 8.13
  },
  {
    "country": "IL",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 79,
    "new_deaths": 1,
    "new_recovered": 74,
    "new_cases_percentage": 0.53,
    "new_deaths_percentage": 0.52,
    "new_recovered_percentage": 1.32
  },
  {
    "country": "BG",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 74,
    "new_deaths": 0,
    "new_recovered": 3,
    "new_cases_percentage": 6.75,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 1.58
  },
  {
    "country": "KZ",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 45,
    "new_deaths": 2,
    "new_recovered": 0,
    "new_cases_percentage": 1.97,
    "new_deaths_percentage": 10,
    "new_recovered_percentage": 0
  },
  {
    "country": "HN",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 43,
    "new_deaths": 0,
    "new_recovered": 19,
    "new_cases_percentage": 8.29,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 61.29
  },
  {
    "country": "GT",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 42,
    "new_deaths": 1,
    "new_recovered": 5,
    "new_cases_percentage": 12.28,
    "new_deaths_percentage": 10,
    "new_recovered_percentage": 20
  },
  {
    "country": "NO",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 40,
    "new_deaths": 1,
    "new_recovered": 0,
    "new_cases_percentage": 0.54,
    "new_deaths_percentage": 0.52,
    "new_recovered_percentage": 0
  },
  {
    "country": "CA",
    "last_update": "2020-04-24T06:31:21",
    "new_cases": 32,
    "new_deaths": 2,
    "new_recovered": 200,
    "new_cases_percentage": 0.07,
    "new_deaths_percentage": 0.09,
    "new_recovered_percentage": 1.34
  },
  {
    "country": "BO",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 31,
    "new_deaths": 3,
    "new_recovered": 0,
    "new_cases_percentage": 4.61,
    "new_deaths_percentage": 7.5,
    "new_recovered_percentage": 0
  },
  {
    "country": "KG",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 25,
    "new_deaths": 0,
    "new_recovered": 20,
    "new_cases_percentage": 3.96,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 6.62
  },
  {
    "country": "UZ",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 20,
    "new_deaths": 0,
    "new_recovered": 2,
    "new_cases_percentage": 1.14,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0.36
  },
  {
    "country": "RE",
    "last_update": "2020-03-21T13:13:35",
    "new_cases": 17,
    "new_deaths": 0,
    "new_recovered": 0,
    "new_cases_percentage": 60.71,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0
  },
  {
    "country": "CD",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 17,
    "new_deaths": 0,
    "new_recovered": 1,
    "new_cases_percentage": 4.51,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 2.13
  },
  {
    "country": "TH",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 15,
    "new_deaths": 0,
    "new_recovered": 60,
    "new_cases_percentage": 0.53,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 2.47
  },
  {
    "country": "VE",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 13,
    "new_deaths": 0,
    "new_recovered": 4,
    "new_cases_percentage": 4.36,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 3.28
  },
  {
    "country": "LT",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 12,
    "new_deaths": 0,
    "new_recovered": 31,
    "new_cases_percentage": 0.86,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 7.77
  },
  {
    "country": "SD",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 12,
    "new_deaths": 3,
    "new_recovered": 0,
    "new_cases_percentage": 7.41,
    "new_deaths_percentage": 23.08,
    "new_recovered_percentage": 0
  },
  {
    "country": "HT",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 10,
    "new_deaths": 1,
    "new_recovered": 0,
    "new_cases_percentage": 16.13,
    "new_deaths_percentage": 25,
    "new_recovered_percentage": 0
  },
  {
    "country": "MQ",
    "last_update": "2020-03-20T10:13:37",
    "new_cases": 9,
    "new_deaths": 0,
    "new_recovered": 0,
    "new_cases_percentage": 39.13,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0
  },
  {
    "country": "NE",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 9,
    "new_deaths": 2,
    "new_recovered": 63,
    "new_cases_percentage": 1.36,
    "new_deaths_percentage": 9.09,
    "new_recovered_percentage": 32.64
  },
  {
    "country": "GP",
    "last_update": "2020-03-21T13:13:35",
    "new_cases": 8,
    "new_deaths": 0,
    "new_recovered": 0,
    "new_cases_percentage": 17.78,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0
  },
  {
    "country": "UY",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 8,
    "new_deaths": 0,
    "new_recovered": 17,
    "new_cases_percentage": 1.46,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 5.04
  },
  {
    "country": "FR",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 7,
    "new_deaths": 0,
    "new_recovered": 11,
    "new_cases_percentage": 0,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0.03
  },
  {
    "country": "BF",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 7,
    "new_deaths": 2,
    "new_recovered": 21,
    "new_cases_percentage": 1.15,
    "new_deaths_percentage": 5.13,
    "new_recovered_percentage": 5.4
  },
  {
    "country": "MM",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 7,
    "new_deaths": 0,
    "new_recovered": 0,
    "new_cases_percentage": 5.3,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0
  },
  {
    "country": "PY",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 7,
    "new_deaths": 0,
    "new_recovered": 3,
    "new_cases_percentage": 3.29,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 4.48
  },
  {
    "country": "KR",
    "last_update": "2020-04-24T06:30:32",
    "new_cases": 6,
    "new_deaths": 0,
    "new_recovered": 90,
    "new_cases_percentage": 0.06,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 1.07
  },
  {
    "country": "CN",
    "last_update": "2020-04-24T01:43:34",
    "new_cases": 6,
    "new_deaths": 0,
    "new_recovered": 46,
    "new_cases_percentage": 0.01,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 0.06
  },
  {
    "country": "HK",
    "last_update": "2020-03-10T23:53:02",
    "new_cases": 5,
    "new_deaths": 0,
    "new_recovered": 6,
    "new_cases_percentage": 4.35,
    "new_deaths_percentage": 0,
    "new_recovered_percentage": 10.17
  }
 ]
 ';
```

To create a Sql statement ready to process this you can run:

`dotnet JsonResponseToSqlQuery.dll --json-response-file "/tmp/Covid.json" --array-name ""`

Which would then place the following Sql Query on the console:

```sql
Select   *
  From   OpenJson(@Json)
    With  (
             [Country]                           NVarChar(4000)          '$.country'
            ,[Last_Update]                       DateTime2(7)            '$.last_update'
            ,[New_Cases]                         BigInt                  '$.new_cases'
            ,[New_Deaths]                        BigInt                  '$.new_deaths'
            ,[New_Recovered]                     BigInt                  '$.new_recovered'
            ,[New_Cases_Percentage]              Numeric(8, 4)           '$.new_cases_percentage'
            ,[New_Deaths_Percentage]             Numeric(8, 4)           '$.new_deaths_percentage'
            ,[New_Recovered_Percentage]          Numeric(8, 4)           '$.new_recovered_percentage'
          ) As JsonQuery;

```

# Overrides

Various overrides can be specified, for example

`dotnet JsonResponseToSqlQuery.dll --json-response-file "/tmp/Covid.json" --array-name "" --default-integer-data-type Integer`

Would give you:

```sql
Select   *
  From   OpenJson(@Json)
    With  (
             [Country]                            NVarChar(4000)          '$.country'
            ,[Last_Update]                        DateTime2(7)            '$.last_update'
            ,[New_Cases]                          Integer                 '$.new_cases'
            ,[New_Deaths]                         Integer                 '$.new_deaths'
            ,[New_Recovered]                      Integer                 '$.new_recovered'
            ,[New_Cases_Percentage]               Numeric(8, 4)           '$.new_cases_percentage'
            ,[New_Deaths_Percentage]              Numeric(8, 4)           '$.new_deaths_percentage'
            ,[New_Recovered_Percentage]           Numeric(8, 4)           '$.new_recovered_percentage'
          ) As JsonQuery;

```

Individual elements can be overriden by virtue of an override file, e.g.

```
# My override file for Covid-19

#Country should be just 2 characters
.country                  |> NChar(2)
# Last Update can be just a date
.last_update              |> Date
# Don't care about recovered numbers
.new_recovered            |> *
.new_recovered_percentage |> *
```
`dotnet JsonResponseToSqlQuery.dll --json-response-file "/tmp/Covid.json" --array-name "" --default-integer-data-type Integer --override-mapping-file /tmp/CovidOverrides.map`

```sql
Select   *
  From   OpenJson(@Json)
    With  (
             [Country]                            NChar(2)                '$.country'
            ,[Last_Update]                        Date                    '$.last_update'
            ,[New_Cases]                          Integer                 '$.new_cases'
            ,[New_Deaths]                         Integer                 '$.new_deaths'
            ,[New_Cases_Percentage]               Numeric(8, 4)           '$.new_cases_percentage'
            ,[New_Deaths_Percentage]              Numeric(8, 4)           '$.new_deaths_percentage'
          ) As JsonQuery;
```

# Saving the Sql Query to a file

Instead of writing the query to the console, you can use the `--sql-output-file` argument.
