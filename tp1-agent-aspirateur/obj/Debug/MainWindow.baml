   M S B A M L   `   `   `  ����  tp1_agent_aspirateur%    tp1_agent_aspirateur.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  Agent Aspirateur��5   	   $	��400��6"   $	��500��6/   .��5      � -   5        $��RoomGrid��5       � ShowGridLines .  � ColumnDefinitions 5	   
   �� 5
      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      $
��Auto �6    � RowDefinitions 5   
   �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5      �� 5       .+�5"   
   �� $��images/wall-e.jpg��6   �� 5#   
   $��0��6   $��10��6"   .+�5$      �� -   5        $��TurnText��5$      $��Right=�6(   
- Tour : -�� 5&      05(      5)      �� 5*   
   $��1��6   $��10��6"   .+�5+      �� -   5        $��PerformanceText��5+      $��Right=�6/   - Performance : -�� 5-      1005/      50      �� 51   
   $��2��6   $��10��6"   .+�52      �� -   5        $��BatteryText��52      $��Right=�6+   - Battery : -�� 54      10056      57      �� 58   
   $��3��6   $��10��6"   .+�59      �� -   5        $��ExplorationText��59      $��Right=�6/   - Exploration : -�� 5;      BFS5=      5>      �� 5?   
   $��4��6   $��10��6"   .+�5@      �� -   5        $��
ActionText��5@      $��Right=�6*   - Action : -�� 5B      STAY5D      5E      �� 5F   
   $��9��6   $��10��6"   .+�5G      �� $��Right=�6   "Espace" pour�� 5I      changer d'algorithme5L      5M      �� 5N   
   $��5��6   $��10��6"   .+�5O      �� -   5        $��LearningText��5O      $��Right=�6/   - nOfActions : -�� 5Q      15S      5T      5U      5V      