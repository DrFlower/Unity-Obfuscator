# Unity-Obfuscator


## 简介
- 这是一款基于Mono Cecil库对C#编译后程序集进行IL代码注入实现的Unity混淆插件
- 混淆发生在编译后，开发过程无需关心
- 此插件已在带Lua热更方案的商业MMO手游项目上验证过可行性


## 主要功能：
1. 对命名空间、类、属性、字段的命名混淆
2. 插入垃圾代码（完善中）
3. 在原代码中插入对垃圾代码的调用

## 特色功能：
1. 命名混淆支持词库抽取名字
2. 支持随机种子，可实现重复Build后混淆结果一致
3. 支持黑白名单，排除部分在混淆后不能正常工作的代码，确保项目正常运行

## 使用方法：
使用时请把UnityObfuscator文件夹放置到Unity工程Assets目录下，在Unity Editor中对Assets\UnityObfuscator\Editor\ObfuscatorConfig.asset文件进行配置，并针对项目具体情况对黑白名单进行配置，配置完后直接Build程序即可生效。

## 插件配置：
![ObfuscatorConfig图片][1]

 - Enable Code Obfuscator控制总混淆开关
 - 可选择使用时间戳作为随机种子或自行控制随机种子
 - 可分别控制**混淆名字**/**垃圾代码**的开关
 - 可分别控制**混淆名字**/**垃圾代码**使用的**黑白名单模式**
 - 可控制生成垃圾方法数量和调用垃圾方法数量（谨慎设置，会对Build的速度以及程序运行速度产生影响，影响大小随垃圾代码库变化）

## 黑白名单配置：

### 模式说明
提供黑名单、白名单、黑白名单混用3种模式。
>- 黑名单模式为只混淆黑名单中的内容；
>- 白名单模式为除了白名单内其他全部混淆；
>- 两者混用模式是在黑名单范围内进行混淆，但会把其中的白名单内容排除在外。


### 配置文件
配置文件位于Assets\UnityObfuscator\Editor\Res目录下

    配置文件以``A-B-C``方式命名
    A为ObfuscateList/WhiteList表示 黑名单/白名单
    B为CodeInject/NameObfuscate表示该名单是控制 代码插入/名字混淆
    C则表示此配置控制的 具体范围

> ObfuscateList``//黑名单``
>> ObfuscateList-CodeInject-Class.txt &nbsp; ``//-``
>> ObfuscateList-CodeInject-Method.txt &nbsp; ``//-``
>> ObfuscateList-CodeInject-Namespace.txt &nbsp; ``//-``
>> ObfuscateList-NameObfuscate-Class.txt &nbsp; ``//名单内的类（包括类名和类成员名）都会被混淆``
>> ObfuscateList-NameObfuscate-ClassExceptClassName.txt &nbsp; ``//名单内的类的类成员名会被混淆，但类名不混淆``
>> ObfuscateList-NameObfuscate-ClassMember.txt &nbsp; ``//名单内的类成员名会被混淆``
>> ObfuscateList-NameObfuscate-Method.txt &nbsp; ``//名单内的方法名会被混淆``
>> ObfuscateList-NameObfuscate-Namespace.txt &nbsp; ``//名单内命名空间里内容包括（命名空间名、类名、类成员名）都会被混淆``
>> ObfuscateList-NameObfuscate-NamespaceExceptNamespaceName.txt &nbsp; ``//名单内命名空间内容（包括类名、类成员名）都会被混淆，但命名空间名不混淆``

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
（待完善）

##混淆结果的验证：
![ILSpy图片][2]
可通过``ILSpy``等反编译软件验证混淆后的程序集
以PC平台为例，Unity Build后的程序集位于目标目录/XX_Data/Managed文件夹中，Unity默认程序集为Assembly-CSharp

## Demo
Demo链接：https://github.com/DrFlower/Unity-Obfuscator-demo

## TODO
- [ ] 完善插入垃圾代码功能
- [ ] 命名混淆支持随机字符
- [ ] 支持多DLL


  [1]: https://github.com/DrFlower/Unity-Obfuscator/blob/master/Doc/ObfuscatorConfig.png
  [2]: https://github.com/DrFlower/Unity-Obfuscator/blob/master/Doc/ILSpy.png