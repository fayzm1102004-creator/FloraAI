import 'package:easy_localization/easy_localization.dart';

Map<String, Map<String, String>> get diseaseInfo => {
  'healthy': {
    'disease': 'disease_healthy'.tr(),
    'treatment': 'treatment_healthy'.tr(),
    'plantType': 'Plant',
    'conditionName': 'Healthy',
    'detectedCategory': 'healthy',
  },
  'fungi': {
    'disease': 'disease_fungi'.tr(),
    'treatment': 'treatment_fungi'.tr(),
    'plantType': 'Plant',
    'conditionName': 'Fungal Infection',
    'detectedCategory': 'fungi',
  },
  'bacteria': {
    'disease': 'disease_bacteria'.tr(),
    'treatment': 'treatment_bacteria'.tr(),
    'plantType': 'Plant',
    'conditionName': 'Bacterial Infection',
    'detectedCategory': 'bacteria',
  },
  'virus': {
    'disease': 'disease_virus'.tr(),
    'treatment': 'treatment_virus'.tr(),
    'plantType': 'Plant',
    'conditionName': 'Viral Infection',
    'detectedCategory': 'virus',
  },
  'pests': {
    'disease': 'disease_pests'.tr(),
    'treatment': 'treatment_pests'.tr(),
    'plantType': 'Plant',
    'conditionName': 'Pest Infestation',
    'detectedCategory': 'pests',
  },
};
