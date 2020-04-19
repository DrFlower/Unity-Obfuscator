# Unity-Obfuscator

## 目录

- [简介](#简介)
- [主要功能](#主要功能)
- [特色功能](#特色功能)
- [使用方法](#使用方法)
- [插件配置](#插件配置)
- [黑白名单配置](#黑白名单配置)
    - [模式说明](#模式说明)
    - [配置文件](#配置文件)
    - [配置格式](#配置格式)
    - [配置规则](#配置规则)
- [垃圾代码库配置](#垃圾代码库配置)
- [混淆结果的验证](#混淆结果的验证)
- [Demo](#Demo)


## 简介
- 这是一款基于Mono.Cecil库对C#编译后程序集进行IL代码注入实现的轻量Unity混淆插件
- 混淆发生在编译后，开发过程无需关心
- 此插件已在带Lua热更方案的商业MMO手游项目上验证过可行性


## 主要功能：
1. 对命名空间、类、属性、字段的命名混淆
2. 插入垃圾代码（完善中）
3. 在原代码中插入对垃圾代码的调用

## 特色功能：
1. 命名混淆支持随机字符串或从词库抽取名字
2. 支持随机种子，可实现重复Build后混淆结果一致
3. 支持黑白名单，排除部分在混淆后不能正常工作的代码，确保项目正常运行
4. 混淆后输出混淆前后的名字对应关系文件
5. 支持多DLL

## 使用方法：
使用时请把UnityObfuscator文件夹放置到Unity工程Assets目录下，在Unity Editor中对Assets\UnityObfuscator\Editor\ObfuscatorConfig.asset文件进行配置，并针对项目具体情况对黑白名单进行配置，配置完后直接Build程序即可生效。

## 插件配置：
![ObfuscatorConfig图片][1]

 - Enable Code Obfuscator控制总混淆开关
 - 可在Random Seed输入混淆用的随机种子的值，也可以勾选Use Time Stamp使用时间戳作为随机种子
 - 可分别控制**命名混淆(Obfuscate Name)**/**代码注入(Inject Code)**的开关
 - 命名混淆
    - Filter Type: 命名混淆用的黑白名单模式，模式区别具体看下文黑白名单配置
    - Name Source：命名来源，可选Random和Word Library两个模式，Random为随机字符串命名，Word Library为从词库中抽取名字作为命名
 - 代码注入
    - Filter Type: 代码注入用的黑白名单模式，模式区别具体看下文黑白名单配置
    - Generate Useless Method Multiple: 生成垃圾方法的倍数。例如填2，A类内原本有3个方法，则会生成6个垃圾方法在此类中``（注意垃圾代码生成不受黑白名单影响，只要开启了代码注入，就会根据这个参数在所有类中插入垃圾代码）``
    - Call Useless Method Per Method: 在每个方法中插入对垃圾代码的调用的数量。例如填1，则每个方法会随机调用1个垃圾方法``(若无特殊需求，谨慎使用，可填0。调用垃圾方法可能会对程序运行性能产生明显影响，具体影响大小由插入数量和垃圾代码库中方法复杂度决定，受黑白名单控制，建议用黑名单模式，仅在性能不敏感的地方插入调用)``
 - Useless Code Library Path: 垃圾代码库路径，默认为Assets/UnityObfuscator/GarbageCode/GarbageCode.dll
 - DLL Path Setting: 需要混淆的DLL的路径，Unity默认生成的DLL在工程目录下的Library/ScriptAssemblies路径中，默认程序集为Assembly-CSharp.dll。``(若项目存在多个DLL，且存在情况：A.dll被混淆，B.dll需要调用A.dll的代码，则B.dll也必须添加到混淆列表中，以修改掉调用的名字，若B.dll本身不需要被混淆则可通过黑白名单来控制)``
 - Test: 提供直接混淆功能，无需Build就可以输出混淆后文件，提高调试效率，混淆后文件输出到Output Path中


>- 命名混淆中Name Source的Word Library的词库文件路径为UnityObfuscator/Editor/Res/NameList.txt，可自行替换词库，按每个名字一行的格式即可，注意词库中的名字**不能重复**
>- 若需要使用代码注入功能，请自行准备垃圾代码库，本插件提供了默认的垃圾代码库模版，只作演示


## 黑白名单配置：

### 模式说明
提供黑名单、白名单、黑白名单混用3种模式。
>- 黑名单模式为只混淆黑名单中的内容；
>- 白名单模式为除了白名单内其他全部混淆；
>- 两者混用模式是在黑名单范围内进行混淆，但会把其中的白名单内容排除在外。


### 配置文件
配置文件位于Assets\UnityObfuscator\Editor\Res目录下

配置文件以**A-B-C**方式命名  
**A**为**ObfuscateList**/**WhiteList**表示 **黑名单**/**白名单**  
**B**为**CodeInject**/**NameObfuscate**表示该名单是控制 **代码插入**/**名字混淆**  
**C**则表示此配置控制的 **具体范围**  


> ObfuscateList``//黑名单``
>> ObfuscateList-CodeInject-Class.txt &nbsp; ``//-``  
>> ObfuscateList-CodeInject-Method.txt &nbsp; ``//-``  
>> ObfuscateList-CodeInject-Namespace.txt &nbsp; ``//-``  
>> ObfuscateList-NameObfuscate-Class.txt &nbsp; ``//名单内的类（包括类名和类成员名）都会被混淆``  
>> ObfuscateList-NameObfuscate-ClassExceptClassName.txt &nbsp; ``//名单内的类的类成员名会被混淆，但类名不混淆``  
>> ObfuscateList-NameObfuscate-ClassMember.txt &nbsp; ``//名单内的类成员名会被混淆``  
>> ObfuscateList-NameObfuscate-Method.txt &nbsp; ``//名单内的方法名会被混淆``  
>> ObfuscateList-NameObfuscate-Namespace.txt &nbsp; ``//名单内命名空间里内容包括（命名空间名、类名、类成员名）都会被混淆``  
>> ObfuscateList-NameObfuscate-NamespaceExceptNamespaceName.txt &nbsp;   ``//名单内命名空间内容（包括类名、类成员名）都会被混淆，但命名空间名不混淆``  

> WhiteList``//白名单``
>> WhiteList-CodeInject-Class.txt &nbsp; ``//-``  
>> WhiteList-CodeInject-Method.txt &nbsp; ``//-``  
>> WhiteList-CodeInject-Namespace.txt &nbsp; ``//-``  
>> WhiteList-NameObfuscate-Class.txt &nbsp; ``//名单内的类（包括类名和类成员）不混淆``  
>> WhiteList-NameObfuscate-ClassMember.txt &nbsp; ``//名单内的类成员不混淆``  
>> WhiteList-NameObfuscate-ClassNameOnly.txt &nbsp; ``//名单内的类的类名不混淆，但类成员混淆``  
>> WhiteList-NameObfuscate-Method.txt &nbsp; ``//名单内的方法名不混淆``  
>> WhiteList-NameObfuscate-NameSpace.txt &nbsp; ``//名单内的命名空间里内容（包括命名空间名、类名、类成员名）都不混淆``  
>> WhiteList-NameObfuscate-NamespaceNameOnly.txt &nbsp; ``//名单内的命名空间的名字不混淆``  

``以上类成员包括字段、属性、方法``



优先级：两者混用模式下白名单比黑名单优先级高  
例如：两者混用模式下，黑名单填写了A命名空间，白名单填写了A命名空间下的B类，那么A命名空间除了B类外其他类都会被混淆。  


### 配置格式

命名空间配置格式：

    UnityEngine
    UnityEngine.UI
    
类配置格式：

    UnityEngine|GameObject
    *|GameObject
    
类成员配置格式：

    UnityEngine|GameObject|name
    UnityEngine|GameObject|AddComponent
    *|GameObject|active
    *|*|active
    *|*|Start
    UnityEngine|GameObject|*

``以上仅为示例，Unity自身库不做混淆``

总体格式为 **命名空间|类名|成员名** ，以|符号分割，其中\*可以表示任意成员，如 **\*|*|Start** 表示任意明明空间下的任意类的Start成员，**UnityEngine|GameObject|\***表示UnityEngine命名空间下的GameObject类里的任意成员，当某一个类没有命名空间时也可以用\*表示。

### 配置规则
>1. Unity的生命周期方法和回调方法不能混淆（插件内部已排除绝大部分Unity生命周期方法和回调方法）。
>2. 直接挂在Prefab上或场景GameObject上的组件类名不能混淆（动态添加组件的可以）。
>3. 诸如UGUI的Button组件的OnClick事件等直接在Inspector面板挂载的方法不能混淆。
>4. 诸如Unity的Invoke、StartCoroutine等通过字符串调用方法的方法名不能混淆。
>5. Lua直接访问C#的不能混淆（通过Wrap注册了映射关系的成员可以）。
>6. 部分涉及反射的代码不能混淆。
>7. 移动平台Native层里直接调用C#或通过Unity内置API发送事件到C#的不能混。
>8. :kissing_heart:``期待您的发现...``


## 垃圾代码库配置：
插件提供了默认的垃圾代码库模版,文件位于Assets/UnityObfuscator/GarbageCode/GarbageCode.dll，里面包含了几个简单的方法，仅作功能演示用，建议需要代码注入功能的请自行准备垃圾代码库。  
垃圾代码库建立应遵循以下规则：  
 - 需要打成DLL文件，且带上.mdb文件在同级目录，若库文件在Unity工程内，应该只勾选Editor平台，避免垃圾库自身被发布出去
 - 垃圾方法应该都处于GarbageCodeLib命名空间下的GarbageCode类下，当然也可以在Define脚本中修改这两个命名
 - 垃圾方法应该声明为public static的，且无参数
 - 垃圾方法内部不能调用方法外任何自定义类、类成员等。

## 混淆结果的验证：
![ILSpy图片][2]
可通过``ILSpy``等反编译软件验证混淆后的程序集  
以PC平台为例，Unity Build后的程序集位于目标目录/XX_Data/Managed文件夹中，Unity默认程序集为Assembly-CSharp

## Demo
Demo链接：https://github.com/DrFlower/Unity-Obfuscator-demo


  [1]: https://github.com/DrFlower/Unity-Obfuscator/blob/master/Doc/ObfuscatorConfig.png
  [2]: https://github.com/DrFlower/Unity-Obfuscator/blob/master/Doc/ILSpy.png
