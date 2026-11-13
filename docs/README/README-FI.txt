HEAT SOUNDTRACK  v1.0
Omat biisisi Need for Speed Heatiin — uusina kappaleina pelin omien rinnalle.
Ilmainen fanityökalu, tekijät Kitivski + Claude. EI yhteydessä Electronic Artsiin tai Ghost Gamesiin.

English — README.txt ohjelman vieressä.
Käännökset tehty tekoälyllä (Claude).

=====================================================================
MITÄ SE TEKEE
  - Pelin kappaleet päälle/pois: talli, kaupunki päivällä, kaupunki yöllä, kisat päivällä, kisat yöllä.
  - Omat biisit (mp3, flac, wav, ogg, m4a) UUSINA kappaleina — rastita, missä kukin soi.
    Pelin kappaleet pysyvät ennallaan.
  - Biisit YouTubesta / YT Musicista: linkki biisiin, soittolistaan tai albumiin, monta linkkiä kerralla tai .txt-lista.
    Jos linkki on musiikkivideo, ohjelma ottaa saman biisin YT Musicista — ilman videon introja ja sketsejä.
  - Kisan lähtö: lähtölaskennan alla soi pelin intro-kappale, biisisi tulevat sen jälkeen. Kisan aloitus omalla biisillä — seuraavassa versiossa.
  - Profiilit: eri kappalevalikoimat, vaihto yhdellä klikkauksella.
  - Rakentaa YHDEN modin Frosty Mod Manageriin. Pelin tiedostoja ei muuteta.
  - Ikkuna puhuu 14 kieltä.

TARVITSET
  - Windows 10 tai 11 (Microsoft Edge on valmiina — ikkuna käyttää sitä).
  - Need for Speed Heat (Steam tai EA app).
  - Frosty Mod Manager, HarGabtin versio NFS Heatille:
    https://github.com/HarGabt/FrostyToolsuite/releases
    Käynnistä se kerran ja valitse peli ennen kuin käytät tätä työkalua.
    Ensimmäisellä käynnistyksellä se kysyy pelin salausavainta — NFS Heat Frosty Key (Nexus Mods).
    https://www.nexusmods.com/needforspeedheat/mods/74   (NFSHEAT.key -> Frosty Mod Manager\)
    Kun painat Launch ensimmäistä kertaa (ja kun ModData-kansio on poistettu), Frosty pyytää
    järjestelmänvalvojan oikeuksia luodakseen linkit pelin tietoihin ("New installation detected") —
    se on normaalia, hyväksy.

NÄIN KÄYTÄT
  1. Pura KOKO zip tavalliseen kansioon (Työpöytä, Tiedostot, D:\Mods…),
     ei Program Filesiin.
  2. Käynnistä "Heat Soundtrack.exe".
  3. Valitse kieli, hyväksy ehdot. Ensimmäinen käynnistys lukee pelin — muutama minuutti, vain kerran.
  4. + LISÄÄ BIISI → valitse tiedosto → rastita, missä se soi → Tallenna.
     Tai ▶ YT MUSICISTA → liitä linkki (biisi, soittolista, albumi) → Lataa.
     Kaikki biisit ovat ohjelman vieressä olevassa "Music"-kansiossa, jokainen omassa numeroidussa kansiossaan:
     Music\1\, Music\2\… — järjestys ikkunassa. + LISÄÄ BIISI luo seuraavan kansion itse.
     Käsin: uusi numeroitu kansio, jossa on biisin tiedosto + kuva (jpg, png) kansikuvaksi.
     Poista kansio — biisi katoaa ohjelmasta.
     Kuva: nimi vapaa, jos se on kansion ainoa kuva; useita — cover.jpg voittaa. Koko vapaa.
  5. RAKENNA MODI. Modi ilmestyy Frosty Mod Managerin listaan.
  6. Frosty Mod Managerissa: rastita "Heat Soundtrack", kytke muut musiikkimodit pois, paina Launch.

VAIN OFFLINE
  Modit ovat yksinpeliin. Ennen kuin menet Online-tilaan, käynnistä peli ilman modeja.
  Online-pelaaminen modien kanssa voi johtaa EA-tilisi sulkemiseen.

"WINDOWS SUOJASI TIETOKONETTASI" / "TUNTEMATON JULKAISIJA"
  Ohjelmaa ei ole allekirjoitettu maksullisella varmenteella, siksi Windows SmartScreen varoittaa siitä.
  Klikkaa "Lisätietoja" → "Suorita silti". Lähdekoodi on "source"-kansiossa.

MITÄ KANSIOSSA ON
  Heat Soundtrack.exe      ohjelma
  app\web                  ikkuna (sivut, tekstit 14 kielellä)
  app\frosty               Frosty Toolsuiten kirjastot — lukevat pelin, kirjoittavat modin (GPL v3)
  app\encoder              EA:n äänienkooderi Frosty Editorin Sound Editor -lisäosasta
  app\yt-dlp.exe           lataa biisejä YouTubesta / YT Musicista (Unlicense)
  source\                  tämän ohjelman lähdekoodi (GPL v3)
  README\                  tämä teksti muilla kielillä
  data\                    syntyy ensimmäisellä käynnistyksellä: asetuksesi, biisit, SINUN pelistäsi luetut kuvat, loki
  Music\                   biisisi, kukin omassa numeroidussa kansiossaan: biisin tiedosto, kansikuva, song.json (esittäjä, nimi)
  THIRD-PARTY.txt          mitä käytämme, tekijät, lisenssit, linkit
  Mukana ei ole pelin eikä EA:n tiedostoja: taustat luetaan omasta pelistäsi.

JOS JOKIN EI TOIMI
  - Loki on data\log.txt (painike "Loki" ikkunassa).
  - Pelin päivityksen jälkeen työkalu lukee pelin uudelleen itse. Jos rakentaminen epäonnistuu viestillä
    "Peli on muuttunut" — etsi työkalusta uudempaa versiota.
  - Aloita alusta: sulje ohjelma ja poista "data"-kansio.

POISTO
  Kytke modi pois Frosty Mod Managerissa ja poista tämä kansio.

---------------------------------------------------------------------
Need for Speed ja NFS Heat ovat Electronic Arts Inc. -yhtiön tavaramerkkejä.
Tämä työkalu ei ole Electronic Artsin tai Ghost Gamesin tekemä, hyväksymä tai tukema.
Tekijät ja lisenssit: THIRD-PARTY.txt ja ohjelman "Kiitokset"-painike.
