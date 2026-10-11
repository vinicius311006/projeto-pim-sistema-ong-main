// This is a basic Flutter widget test.
//
// To perform an interaction with a widget in your test, use the WidgetTester
// utility in the flutter_test package. For example, you can send tap and scroll
// gestures. You can also use WidgetTester to find child widgets in the widget
// tree, read text, and verify that the values of widget properties are correct.

import 'package:flutter_test/flutter_test.dart';

import 'package:vestindo_sonhos_mobile/main.dart';

void main() {
  testWidgets('App renders login or home screen', (WidgetTester tester) async {
    await tester.pumpWidget(const VestindoSonhosApp(inicialmenteLogado: false));
    expect(find.byType(VestindoSonhosApp), findsOneWidget);
  });
}
