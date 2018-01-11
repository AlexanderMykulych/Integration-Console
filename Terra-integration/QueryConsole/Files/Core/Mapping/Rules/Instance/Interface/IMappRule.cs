namespace Terrasoft.TsIntegration.Configuration{
	public interface IMappRule
	{
		void Import(RuleImportInfo info);
		void Export(RuleExportInfo info);
	}
}