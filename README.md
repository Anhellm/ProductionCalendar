# Решение CalendarExtensions

## Модуль "Производственный календарь"
### Описание

Модуль предназначен для обновления данных по выходным и праздничным дням из внешних сервисов и отображения данной информации.

### Функциональные возможности

#### **Обложка.**
Предоставляет доступ для работы с общими и отдельно частными календарями рабочего времени, создавая надстройку для взаимодействия по обновлению и визуальному представлению данных.

#### **Справочник "Настройки".**
Предназначен для определения общих настроек работы с модулем. 
Задает общие настройки по графику работы и взаимодействию с внешними сервисами.

#### **Справочник "Производственный календарь".**
Обертка для базового справочника "Календарь рабочего времени".
Отображает информацию в виде производственного календаря для существующего календаря рабочего времени. 
Позволяет:
- Обновить данные из внешнего сервиса.
- Изменить график работы за период.

#### **Справочник "Сервис".**
Перечень доступных для использования внешних сервисов и их настройки. Создается программно.

#### **Фоновый процесс "Обновление данных календаря на следующий год".**
Фоновый процесс для автоматизации создания/обновления данных по выходным и праздничным дням на следующий год.

> [NOTE]
> Замечания и пожеланию по развитию шаблона разработки фиксируйте через [Issues](https://github.com/DirectumCompany/rx-template-appliedhttpclient/issues).
При оформлении ошибки, опишите сценарий для воспроизведения. Для пожеланий приведите обоснование для описываемых изменений - частоту использования, бизнес-ценность, риски и/или эффект от реализации.
> 
> Внимание! Изменения будут вноситься только в новые версии.

## Внешняя библиотека
Для расширения функционала взаимодействия с внешними сервисами используется библиотека https://github.com/STARKOV-Group/CalendarService
> Библиотека используется до версии Directum 4.12. Начиная с версии 4.12 для взаимодействия с внешними сервисами используются изолированные области.

## Порядок установки
Для работы требуется установленный Directum Development studio версии 4.5 и выше.

### Установка для ознакомления
1. Склонировать репозиторий CalendarExtensions в папку (например C:\WorkFolder).
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="<адрес локального репозитория>" />
  <repository folderName="Work" solutionType="Work" url="<адрес локального репозитория>" />
  <repository folderName="<Папка из п.1>" solutionType="Work" url="https://github.com/STARKOV-Group/CalendarExtensions" />
</block>
```

### Установка для использования на проекте
Возможные варианты:

#### A. Fork репозитория
1. Сделать fork репозитория CalendarExtensions для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.
3. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="<адрес локального репозитория>" />
  <repository folderName="Work" solutionType="Work" url="<адрес локального репозитория>" />
  <repository folderName="<Папка из п.2>" solutionType="Work" url="<Адрес репозитория gitHub учетной записи пользователя из п. 1>" />
</block>
```

#### B. Подключение на базовый слой.
Вариант не рекомендуется:
* так как при выходе новой версии шаблона разработки не гарантируется обратная совместимость;
* потеряется возможность изменения или доработки функционала под собственые требования;

1. Склонировать репозиторий CalendarExtensions в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Base" url="<Адрес репозитория gitHub>" />
  <repository folderName="<Папка для рабочего слоя>" solutionType="Work" url="https://github.com/STARKOV-Group/CalendarExtensions" />
</block>
```

#### C. Копирование репозитория в систему контроля версий.
Рекомендуемый вариант для проектов внедрения.

1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий CalendarExtensions в папку с ключом ```--mirror```.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:
```
git push –mirror <Адрес репозитория из п. 1>
```
