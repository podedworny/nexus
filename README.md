# NightFall

## 1. Krótki opis gry
**NightFall** to trójwymiarowa gra akcji z elementami survivalu i strzelanki. Głównym zadaniem gracza jest obrona obozowiska i przetrwanie serii nadciągających fal potworów. Główną inspiracją dla mechaniki i struktury rozgrywki był tryb *Ratowanie Świata* z gry *Fortnite*. Rozgrywka opiera się na umiejętnym zarządzaniu zasobami - gracz musi odpierać ataki nocą, aby za dnia móc inwestować zdobyte fundusze w ulepszenia pancerza, zakup nowej broni oraz amunicji. Cechą charakterystyczną projektu jest wyraźny podział na fazę ekonomiczno-przygotowawczą oraz fazę intensywnej walki o przetrwanie.

## 2. Użyte narzędzia
* **Silnik gry:** Unity 3D
* **Język programowania skryptów:** C#
* **Docelowa platforma sprzętowa:** Komputery z systemem Windows

## 3. Opis mechaniki gry
* **Świat gry:** Rozgrywka toczy się w środowisku 3D o ograniczonej, zamkniętej strukturze. Areną działań jest zalesiony teren u podnóża góry, w centrum którego znajduje się obozowisko gracza.
* **Kamera:** Zastosowano hybrydowy system pracy kamery. Domyślnie gracz obserwuje akcję z perspektywy trzeciej osoby, co ułatwia orientację w terenie. W momencie korzystania z broni palnej i celowania, widok płynnie przełącza się na perspektywę pierwszej osoby, co zwiększa precyzję strzału.
* **System walki i ekwipunek:** Gracz rozpoczyna zabawę dysponując jedynie bronią białą (kij baseballowy). Walka w zwarciu oszczędza zasoby, ale naraża na bezpośrednie obrażenia. Wraz z postępem gry odblokowywana jest broń palna (pistolet, karabin), co pozwala na bezpieczniejszą walkę na dystans, wymuszając jednak ostrożne zarządzanie kończącą się amunicją. Dodatkowo broń można trzykrotnie ulepszyć w sklepie, zwiększając jej obrażenia.
* **Statystyki postaci:** 
    * **Zdrowie (HP):** Spada pod wpływem ataków przeciwników, można je regenerować za pomocą bandaży.
  * **Stamina (Kondycja):** Zużywa się podczas sprintu, co wymusza taktyczne podejście do pozycjonowania i ucieczki.
  * **Pancerz:** Statystyka, którą można ulepszać w sklepie. Każdy kolejny poziom pancerza permanentnie zmniejsza obrażenia przyjmowane od ciosów wrogów.
* **Przeciwnicy (Zombie):** Atakują wyłącznie w zwarciu. Gra składa się łącznie z 30 fal. Standardowe potwory pojawiają się co noc. Co 5 rund (5, 15, 25) pojawiają się silniejsze Minibossy, a co 10 rund (10, 20, 30) do walki wkraczają główni Bossowie. Specjalni przeciwnicy charakteryzują się większą pulą zdrowia, wyższymi obrażeniami, unikalną wizualną aurą, ale także znacznie wyższą nagrodą finansową za ich pokonanie.
* **Cykl Dnia i Nocy:** Gra podzielona jest na dwie fazy. *Dzień* jest bezpieczny – gracz przebywa w obozowisku, gdzie może korzystać ze sklepu i ulepszać postać za zdobyte pieniądze. Gdy gracz jest gotowy, ręcznie inicjuje *Noc* (klawisz F przy namiocie), która trwa do momentu pokonania wszystkich potworów z danej fali. Gracz dysponuje 3 życiami na całą grę.
* **Interfejs i sterowanie:** 
    * **HUD:** Ekran wyświetla paski zdrowia i staminy, wskaźnik żyć, aktualny numer fali, stan portfela, pasek wybranej broni (Hotbar) oraz zapas amunicji.
  * **Sterowanie:** Klawisze `W`, `A`, `S`, `D` (poruszanie), `Spacja` (skok), `Shift` (sprint), `E` (otwieranie ekwipunku), `Q` (sklep – dostępny tylko w obozie). Lewy przycisk myszy odpowiada za atak, prawy za celowanie. Zmiana broni następuje za pomocą klawiszy numerycznych (`1`-`5`). `ESC` służy do pauzowania gry i zamykania okien interfejsu.

## 4. Użyte assety
* Modele 3D postaci oraz elementów otoczenia pobrano z oficjalnego sklepu Unity Asset Store.
* Animacje postaci gracza oraz przeciwników zaimportowano z biblioteki Mixamo i zintegrowano z systemem mechaniki.
* Interfejs użytkownika (UI) składa się z elementów wygenerowanych za pomocą wbudowanego systemu Canvas w Unity oraz prostych grafik wykonanych ręcznie w programie GIMP.
* Logo gry zostało wygenerowane za pomocą narzędzi sztucznej inteligencji, a następnie zaimplementowane do menu głównego.

## 5. Wykorzystanie sztucznej inteligencji
Sztuczna inteligencja odegrała istotną rolę w kilku aspektach projektu:
* **Rozwój techniczny i skryptowanie:** Wykorzystano modele językowe LLM jako wsparcie przy projektowaniu logiki kodu, tworzeniu menedżerów rozgrywki oraz optymalizacji skryptów w języku C#.
* **Assety wizualne:** Moduł generujący obrazy AI posłużył do stworzenia głównego loga oraz tła wykorzystywanego w menu gry.
* **Sztuczna inteligencja przeciwników:** Zachowanie zombie opiera się na algorytmach sztucznej inteligencji wbudowanych w silnik Unity. Wykorzystano system nawigacji *NavMesh* do wyznaczania ścieżek patrolowych oraz omijania przeszkód, połączony z klasyczną maszyną stanów, która płynnie steruje przejściami między pościgiem za graczem a atakowaniem.

## 6. Uruchomienie gry
Gra została skompilowana do postaci wykonywalnej dla systemu Windows. Aby rozpocząć rozgrywkę:
1. Rozpakuj pobrane archiwum z grą do dowolnego folderu.
2. Uruchom plik `Nightfall.exe` znajdujący się w głównym katalogu.

## 7. Zrzuty ekranu

*Menu główne gry*

![Menu główne gry](https://i.imgur.com/Ne29F1Y.png)

*Sklep*

![Sklep](https://i.imgur.com/pUnPhXX.png)

*Ekwipunek*

![Ekwpiunek](https://i.imgur.com/K1f0sra.png)

*Walka w nocy*

![Walka w nocy](https://i.imgur.com/JommIxy.png)

## Asset Attribution

This project includes a CC-BY model. Keep the attribution below in derivative
builds and public releases.

Baseball bat by jeremy [CC-BY](https://creativecommons.org/licenses/by/3.0/) via
Poly Pizza: https://poly.pizza/m/9FPflHIzK73
