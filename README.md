# AutoRegistration
Решение для Directum RX 4.10
____
# Возможности решения

Регистрация документов в процессе согласования.
____
# Состав решения
 
-Модуль Авторегистрации.

-Блок авторегистрации для процессов согласования
____

## Порядок установки
Для работы требуется установленный Directum RX версии 4.10 и выше.
1. Склонировать репозиторий [https://github.com/CentrVD-dev/AutoRegistrationBlock](https://github.com/CentrVD-dev/AutoRegistrationBlock/) в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" /> 
  <repository folderName="<Папка из п.1>" solutionType="Work" 
     url="https://github.com/DirectumCompany/rx-template-substitutions.git" />
</block>
```
